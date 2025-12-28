#!/usr/bin/env python3
"""
Decode length-delimited (varint32-prefixed) protobuf .pb files by splitting
messages and calling `protoc --decode_raw` on each message. If the file is not
length-delimited, the script will fall back to running `protoc --decode_raw` on
the whole file.

Usage:
  python Scripts/DecodePbDelimited.py path/to/file.pb > decoded.txt
  python Scripts/DecodePbDelimited.py .sonarqube/out/0/output-cs/log.pb

Requirements:
  - Python 3
  - protoc on PATH

The script prints separators between messages and returns exit code 0 on success.
"""
import sys
import subprocess
import os


def read_varint(data, pos):
    """Read a varint32 from data starting at pos. Returns (value, new_pos).
    Raises ValueError if varint is malformed or truncated.
    """
    value = 0
    shift = 0
    start = pos
    while pos < len(data):
        b = data[pos]
        pos += 1
        value |= (b & 0x7F) << shift
        if not (b & 0x80):
            return value, pos
        shift += 7
        if shift >= 35:
            raise ValueError('Varint too long')
    raise ValueError('Truncated varint starting at %d' % start)


def run_protoc_decode_raw(message_bytes):
    try:
        p = subprocess.run(
            ['protoc', '--decode_raw'],
            input=message_bytes,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )
        stdout = p.stdout.decode('utf-8', errors='replace')
        stderr = p.stderr.decode('utf-8', errors='replace')
        if p.returncode != 0:
            return False, stderr + stdout
        return True, stdout
    except FileNotFoundError:
        return False, 'protoc not found on PATH'


def decode_length_delimited(data):
    pos = 0
    parts = []
    while pos < len(data):
        try:
            size, pos = read_varint(data, pos)
        except ValueError:
            # Not a length-delimited stream (or malformed)
            return None
        if size < 0 or pos + size > len(data):
            return None
        msg = data[pos:pos+size]
        parts.append(msg)
        pos += size
    return parts


def main():
    if len(sys.argv) < 2:
        print('Usage: DecodePbDelimited.py <file.pb>', file=sys.stderr)
        sys.exit(2)

    path = sys.argv[1]
    if not os.path.isfile(path):
        print('File not found: %s' % path, file=sys.stderr)
        sys.exit(2)

    with open(path, 'rb') as f:
        data = f.read()

    # Try length-delimited parsing
    parts = decode_length_delimited(data)
    if parts is None or len(parts) == 0:
        # fallback: try decode_raw on whole file
        ok, out = run_protoc_decode_raw(data)
        if not ok:
            print('Failed to run protoc --decode_raw:')
            print(out, file=sys.stderr)
            sys.exit(1)
        else:
            print('=== decoded (whole file) ===')
            print(out)
            sys.exit(0)

    # decode each part via protoc
    for i, msg in enumerate(parts, start=1):
        print('=== message %d / %d (bytes=%d) ===' % (i, len(parts), len(msg)))
        ok, out = run_protoc_decode_raw(msg)
        if not ok:
            print('--- protoc failed for message %d ---' % i, file=sys.stderr)
            print(out, file=sys.stderr)
            # continue to next message
        else:
            print(out)

    sys.exit(0)


if __name__ == '__main__':
    main()
