#!/usr/bin/env node

import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { RootsListChangedNotificationSchema } from "@modelcontextprotocol/sdk/types.js";
import fs from "fs/promises";
import { createReadStream } from "fs";
import path from "path";
import os from "os";
import { minimatch } from "minimatch";
import { z } from "zod";

const isWindows = process.platform === "win32";

function normalizePath(p) {
    // Normalize separators and remove trailing slashes.
    let normalized = path.normalize(p);
    while (normalized.length > 1 && (normalized.endsWith(path.sep) || normalized.endsWith("/"))) {
        normalized = normalized.slice(0, -1);
    }
    return normalized;
}

function expandHome(p) {
    if (p === "~") return os.homedir();
    if (p.startsWith("~/") || p.startsWith("~\\")) return path.join(os.homedir(), p.slice(2));
    return p;
}

function parseFileUriToPath(uri) {
    // Handles VS Code Windows roots like: file:///c%3A/Users/... (colon percent-encoded)
    // and normal file URIs like: file:///c:/Users/...
    try {
        const u = new URL(uri);
        if (u.protocol !== "file:") return null;

        // URL.pathname is already slash-separated; decode percent-encoding.
        let pathname = decodeURIComponent(u.pathname);

        // On Windows, file URIs commonly come as /c:/... (leading slash before drive).
        if (isWindows && /^\/[a-zA-Z]:\//.test(pathname)) {
            pathname = pathname.slice(1);
        }

        // Convert forward slashes to platform separators for resolve.
        const platformPath = isWindows ? pathname.replaceAll("/", "\\") : pathname;
        return platformPath;
    } catch {
        return null;
    }
}

function parseRootUri(rootUri) {
    if (!rootUri) return null;

    if (rootUri.startsWith("file://")) {
        return parseFileUriToPath(rootUri);
    }

    // Some clients may send plain paths.
    return rootUri;
}

async function resolveExistingDirectory(dir) {
    const expanded = expandHome(dir);
    const absolute = path.resolve(expanded);
    const real = await fs.realpath(absolute);
    const stats = await fs.stat(real);
    if (!stats.isDirectory()) throw new Error(`${dir} is not a directory`);
    return normalizePath(real);
}

async function getValidRootDirectories(requestedRoots) {
    const validated = [];

    for (const root of requestedRoots ?? []) {
        const raw = parseRootUri(root?.uri ?? "");
        if (!raw) continue;

        try {
            const expanded = expandHome(raw);
            const absolute = path.resolve(expanded);
            const real = await fs.realpath(absolute);
            const stats = await fs.stat(real);
            if (stats.isDirectory()) {
                validated.push(normalizePath(real));
            }
        } catch {
            // Intentionally ignore invalid roots to avoid noisy logs.
        }
    }

    return validated;
}

function isWithinAllowedDirectories(candidatePath, allowedDirectories) {
    const candidate = normalizePath(candidatePath);
    const candCmp = isWindows ? candidate.toLowerCase() : candidate;

    for (const allowedDir of allowedDirectories) {
        const base = normalizePath(allowedDir);
        const baseCmp = isWindows ? base.toLowerCase() : base;

        if (candCmp === baseCmp) return true;

        const prefix = baseCmp.endsWith(path.sep) ? baseCmp : baseCmp + path.sep;
        if (candCmp.startsWith(prefix)) return true;
    }

    return false;
}

async function resolveAndValidatePath(inputPath, allowedDirectories) {
    const raw = parseRootUri(inputPath) ?? inputPath;
    const expanded = expandHome(raw);

    const resolved = path.isAbsolute(expanded)
        ? path.resolve(expanded)
        : path.resolve(allowedDirectories[0] ?? process.cwd(), expanded);

    // Resolve symlinks where possible; for paths that don't exist yet (write), resolve parent.
    let finalPath;
    try {
        finalPath = await fs.realpath(resolved);
    } catch {
        const parent = path.dirname(resolved);
        const parentReal = await fs.realpath(parent);
        finalPath = path.join(parentReal, path.basename(resolved));
    }

    if (!isWithinAllowedDirectories(finalPath, allowedDirectories)) {
        throw new Error("Access denied: Path is outside allowed directories.");
    }

    return finalPath;
}

async function readFileAsBase64(filePath) {
    return await new Promise((resolve, reject) => {
        const stream = createReadStream(filePath);
        const chunks = [];
        stream.on("data", (chunk) => chunks.push(chunk));
        stream.on("end", () => resolve(Buffer.concat(chunks).toString("base64")));
        stream.on("error", reject);
    });
}

function headLines(text, n) {
    const lines = text.split(/\r?\n/);
    return lines.slice(0, n).join("\n");
}

function tailLines(text, n) {
    const lines = text.split(/\r?\n/);
    return lines.slice(Math.max(0, lines.length - n)).join("\n");
}

function formatListing(entries) {
    return entries
        .map((entry) => `${entry.isDirectory() ? "[DIR]" : "[FILE]"} ${entry.name}`)
        .join("\n");
}

function toTextResult(text) {
    return {
        content: [{ type: "text", text }],
        structuredContent: { content: text },
    };
}

const args = process.argv.slice(2);

let allowedDirectories = [];
if (args.length > 0) {
    allowedDirectories = await Promise.all(args.map(resolveExistingDirectory));
}

const server = new McpServer({ name: "filesystem-win", version: "0.1.0" });

async function updateAllowedDirectoriesFromRoots(requestedRoots) {
    const validated = await getValidRootDirectories(requestedRoots);
    if (validated.length > 0) {
        allowedDirectories = validated;
    }
}

server.server.oninitialized = async () => {
    const caps = server.server.getClientCapabilities();
    if (caps?.roots) {
        try {
            const response = await server.server.listRoots();
            if (response && "roots" in response) {
                await updateAllowedDirectoriesFromRoots(response.roots);
            }
        } catch {
            // ignore
        }
    }

    if (allowedDirectories.length === 0) {
        throw new Error(
            "Server cannot operate: no allowed directories. Configure via args or ensure client roots are provided."
        );
    }
};

server.server.setNotificationHandler(RootsListChangedNotificationSchema, async () => {
    try {
        const response = await server.server.listRoots();
        if (response && "roots" in response) {
            await updateAllowedDirectoriesFromRoots(response.roots);
        }
    } catch {
        // ignore
    }
});

server.registerTool(
    "list_allowed_directories",
    {
        title: "List Allowed Directories",
        description: "Returns the list of directories that this server is allowed to access.",
        inputSchema: {},
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async () => toTextResult(`Allowed directories:\n${allowedDirectories.join("\n")}`)
);

const ReadTextFileArgsSchema = z.object({
    path: z.string(),
    tail: z.number().optional(),
    head: z.number().optional(),
});

server.registerTool(
    "read_text_file",
    {
        title: "Read Text File",
        description: "Read a file as UTF-8 text (optionally head/tail). Only works within allowed directories.",
        inputSchema: ReadTextFileArgsSchema.shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        if (args.head && args.tail) throw new Error("Cannot specify both head and tail.");
        const filePath = await resolveAndValidatePath(args.path, allowedDirectories);
        const content = await fs.readFile(filePath, "utf8");
        const text = args.head ? headLines(content, args.head) : args.tail ? tailLines(content, args.tail) : content;
        return toTextResult(text);
    }
);

server.registerTool(
    "read_file",
    {
        title: "Read File (Deprecated)",
        description: "Alias for read_text_file.",
        inputSchema: ReadTextFileArgsSchema.shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        return await server.server.callTool({ name: "read_text_file", arguments: args });
    }
);

const ReadMultipleFilesArgsSchema = z.object({
    paths: z.array(z.string()).min(1),
});

server.registerTool(
    "read_multiple_files",
    {
        title: "Read Multiple Files",
        description: "Read multiple files as UTF-8 text.",
        inputSchema: ReadMultipleFilesArgsSchema.shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        const results = await Promise.all(
            args.paths.map(async (p) => {
                try {
                    const filePath = await resolveAndValidatePath(p, allowedDirectories);
                    const content = await fs.readFile(filePath, "utf8");
                    return `${p}:\n${content}\n`;
                } catch (e) {
                    const msg = e instanceof Error ? e.message : String(e);
                    return `${p}: Error - ${msg}`;
                }
            })
        );

        return toTextResult(results.join("\n---\n"));
    }
);

const ReadMediaFileArgsSchema = z.object({ path: z.string() });
server.registerTool(
    "read_media_file",
    {
        title: "Read Media File",
        description: "Read an image/audio file and return base64 data.",
        inputSchema: ReadMediaFileArgsSchema.shape,
        outputSchema: { content: z.array(z.object({ type: z.string(), data: z.string(), mimeType: z.string() })) },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        const filePath = await resolveAndValidatePath(args.path, allowedDirectories);
        const ext = path.extname(filePath).toLowerCase();
        const mime =
            {
                ".png": "image/png",
                ".jpg": "image/jpeg",
                ".jpeg": "image/jpeg",
                ".gif": "image/gif",
                ".webp": "image/webp",
                ".bmp": "image/bmp",
                ".svg": "image/svg+xml",
                ".mp3": "audio/mpeg",
                ".wav": "audio/wav",
                ".ogg": "audio/ogg",
                ".flac": "audio/flac",
            }[ext] ?? "application/octet-stream";

        const data = await readFileAsBase64(filePath);
        const type = mime.startsWith("image/") ? "image" : mime.startsWith("audio/") ? "audio" : "blob";

        return {
            content: [{ type, data, mimeType: mime }],
            structuredContent: { content: [{ type, data, mimeType: mime }] },
        };
    }
);

const ListDirectoryArgsSchema = z.object({ path: z.string() });
server.registerTool(
    "list_directory",
    {
        title: "List Directory",
        description: "List directory contents.",
        inputSchema: ListDirectoryArgsSchema.shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        const dirPath = await resolveAndValidatePath(args.path, allowedDirectories);
        const entries = await fs.readdir(dirPath, { withFileTypes: true });
        return toTextResult(formatListing(entries));
    }
);

server.registerTool(
    "list_directory_with_sizes",
    {
        title: "List Directory with Sizes",
        description: "List directory contents including file sizes.",
        inputSchema: z.object({ path: z.string(), sortBy: z.enum(["name", "size"]).optional().default("name") }).shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        const dirPath = await resolveAndValidatePath(args.path, allowedDirectories);
        const entries = await fs.readdir(dirPath, { withFileTypes: true });
        const detailed = await Promise.all(
            entries.map(async (entry) => {
                const p = path.join(dirPath, entry.name);
                try {
                    const st = await fs.stat(p);
                    return { name: entry.name, isDirectory: entry.isDirectory(), size: st.size };
                } catch {
                    return { name: entry.name, isDirectory: entry.isDirectory(), size: 0 };
                }
            })
        );

        detailed.sort((a, b) => {
            if (args.sortBy === "size") return b.size - a.size;
            return a.name.localeCompare(b.name);
        });

        const lines = detailed.map((e) => `${e.isDirectory ? "[DIR]" : "[FILE]"} ${e.name}${e.isDirectory ? "" : ` (${e.size} bytes)`}`);
        return toTextResult(lines.join("\n"));
    }
);

server.registerTool(
    "create_directory",
    {
        title: "Create Directory",
        description: "Create a directory recursively.",
        inputSchema: z.object({ path: z.string() }).shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: false, idempotentHint: true, destructiveHint: false },
    },
    async (args) => {
        const dirPath = await resolveAndValidatePath(args.path, allowedDirectories);
        await fs.mkdir(dirPath, { recursive: true });
        return toTextResult(`Successfully created directory ${args.path}`);
    }
);

server.registerTool(
    "get_file_info",
    {
        title: "Get File Info",
        description: "Get metadata for a file/directory.",
        inputSchema: z.object({ path: z.string() }).shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        const p = await resolveAndValidatePath(args.path, allowedDirectories);
        const st = await fs.stat(p);
        const info = {
            path: p,
            type: st.isDirectory() ? "directory" : "file",
            size: st.size,
            mtime: st.mtime.toISOString(),
            ctime: st.ctime.toISOString(),
        };
        return toTextResult(Object.entries(info).map(([k, v]) => `${k}: ${v}`).join("\n"));
    }
);

server.registerTool(
    "move_file",
    {
        title: "Move File",
        description: "Move/rename a file or directory.",
        inputSchema: z.object({ source: z.string(), destination: z.string() }).shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: false, idempotentHint: false, destructiveHint: false },
    },
    async (args) => {
        const src = await resolveAndValidatePath(args.source, allowedDirectories);
        const dst = await resolveAndValidatePath(args.destination, allowedDirectories);
        await fs.rename(src, dst);
        return toTextResult(`Successfully moved ${args.source} to ${args.destination}`);
    }
);

server.registerTool(
    "write_file",
    {
        title: "Write File",
        description: "Create/overwrite a file with text content.",
        inputSchema: z.object({ path: z.string(), content: z.string() }).shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: false, idempotentHint: true, destructiveHint: true },
    },
    async (args) => {
        const p = await resolveAndValidatePath(args.path, allowedDirectories);
        await fs.mkdir(path.dirname(p), { recursive: true });
        await fs.writeFile(p, args.content, "utf8");
        return toTextResult(`Successfully wrote to ${args.path}`);
    }
);

server.registerTool(
    "edit_file",
    {
        title: "Edit File",
        description: "Apply exact text replacements in a file.",
        inputSchema: z
            .object({
                path: z.string(),
                edits: z.array(z.object({ oldText: z.string(), newText: z.string() })),
                dryRun: z.boolean().default(false),
            })
            .shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: false, idempotentHint: false, destructiveHint: true },
    },
    async (args) => {
        const p = await resolveAndValidatePath(args.path, allowedDirectories);
        const original = await fs.readFile(p, "utf8");
        let updated = original;

        for (const edit of args.edits) {
            if (!updated.includes(edit.oldText)) {
                throw new Error("Edit failed: oldText not found.");
            }
            updated = updated.replace(edit.oldText, edit.newText);
        }

        if (args.dryRun) {
            return toTextResult("Dry run successful (diff output not implemented in this lightweight server).");
        }

        await fs.writeFile(p, updated, "utf8");
        return toTextResult("Edits applied successfully.");
    }
);

server.registerTool(
    "search_files",
    {
        title: "Search Files",
        description: "Search for files/directories matching a glob pattern.",
        inputSchema: z.object({ path: z.string(), pattern: z.string(), excludePatterns: z.array(z.string()).optional().default([]) }).shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        const root = await resolveAndValidatePath(args.path, allowedDirectories);
        const results = [];

        async function walk(current) {
            const entries = await fs.readdir(current, { withFileTypes: true });
            for (const entry of entries) {
                const full = path.join(current, entry.name);
                const rel = path.relative(root, full);

                const excluded = (args.excludePatterns ?? []).some((p) => minimatch(rel, p, { dot: true }));
                if (excluded) continue;

                if (minimatch(rel, args.pattern, { dot: true })) {
                    results.push(full);
                }

                if (entry.isDirectory()) {
                    await walk(full);
                }
            }
        }

        await walk(root);
        return toTextResult(results.length ? results.join("\n") : "No matches found");
    }
);

server.registerTool(
    "directory_tree",
    {
        title: "Directory Tree",
        description: "Return a recursive directory tree as JSON.",
        inputSchema: z.object({ path: z.string(), excludePatterns: z.array(z.string()).optional().default([]) }).shape,
        outputSchema: { content: z.string() },
        annotations: { readOnlyHint: true },
    },
    async (args) => {
        const root = await resolveAndValidatePath(args.path, allowedDirectories);

        async function build(current) {
            const entries = await fs.readdir(current, { withFileTypes: true });
            const out = [];

            for (const entry of entries) {
                const full = path.join(current, entry.name);
                const rel = path.relative(root, full);
                const excluded = (args.excludePatterns ?? []).some((p) => minimatch(rel, p, { dot: true }));
                if (excluded) continue;

                if (entry.isDirectory()) {
                    out.push({ name: entry.name, type: "directory", children: await build(full) });
                } else {
                    out.push({ name: entry.name, type: "file" });
                }
            }

            return out;
        }

        const tree = await build(root);
        return toTextResult(JSON.stringify(tree, null, 2));
    }
);

const transport = new StdioServerTransport();
await server.connect(transport);
