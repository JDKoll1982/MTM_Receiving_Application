// Typst preamble injected by pandoc via --include-in-header (see _output.yaml → includes.in_header).
//
// Markdown pipe tables carry no alignment information unless the delimiter row uses `:---`
// markers, so pandoc emits `align: (auto, ...)`. Inside the template's align(center) wrapper
// that resolves to centred cell text, which is hard to scan across the wide reference tables
// these specs use. Forcing cells to the start edge gives the conventional left-aligned table.
#show table.cell: set align(left)
