---
description: Guidelines for conducting comprehensive research before generating documentation or code artifacts
applyTo: '**/*.md'
---

# Comprehensive Research Before Creation

Instructions for conducting thorough research across multiple authoritative sources before generating documentation, code examples, or other artifacts.

## When to Apply This Workflow

Trigger this comprehensive research workflow when the user requests:
- "Do research"
- "Research first"
- "Look up the latest information"
- "Check current standards"
- "Review best practices online"
- "Get full context"
- "Read from multiple sources"
- Any variation indicating they want thorough background research before creation

## Research Methodology

### Minimum Source Requirements

**For technical topics:** Read from **at least 10 authoritative sources**
**For documentation/standards:** Read from **at least 10 official references**

### Authoritative Source Hierarchy

1. **Primary Sources (Highest Priority)**
   - Official documentation (e.g., Microsoft Learn, MDN, W3C)
   - Standards bodies (e.g., agentskills.io, WHATWG)
   - Official GitHub repositories (e.g., microsoft/*, github/*)
   - Product documentation from original creators

2. **Secondary Sources**
   - Well-maintained community repositories (10k+ stars)
   - Official blogs and engineering posts
   - Technical learning platforms (web.dev, freeCodeCamp)
   - Industry-standard references

3. **Tertiary Sources (Use Sparingly)**
   - Community tutorials (verify against primary sources)
   - Stack Overflow (for patterns, not standards)
   - Medium articles (only from verified experts)

### Research Process

#### Step 1: Source Identification
Identify and prioritize sources based on:
- **Recency** - Prefer sources updated within the last 12 months
- **Authority** - Official documentation over third-party
- **Completeness** - Full specifications over blog posts
- **Reputation** - Established sources with community validation

#### Step 2: Comprehensive Reading
For each topic area (e.g., "Agent Skills", "Modern HTML"):
1. Read the official specification or standard
2. Review official implementation guides
3. Check for recent updates or changes
4. Read community best practices
5. Review example implementations
6. Check for common pitfalls or anti-patterns
7. Note version-specific details
8. Identify deprecated vs current approaches
9. Review accessibility/security considerations
10. Cross-reference multiple sources for consistency

#### Step 3: Context Synthesis
After reading all sources:
1. **Identify Core Concepts** - What are the fundamental principles?
2. **Note Latest Standards** - What changed in recent versions?
3. **Catalog Best Practices** - What do experts recommend?
4. **Document Anti-Patterns** - What should be avoided?
5. **Extract Key Examples** - What are the canonical patterns?
6. **List Required vs Optional** - What's mandatory vs nice-to-have?
7. **Version Mapping** - What features require specific versions?
8. **Browser/Platform Support** - What are compatibility considerations?

#### Step 4: Summary Creation
Create a structured summary including:

```markdown
## RESEARCH SUMMARY - READY FOR CREATION

### [Topic Area 1] Research (10+ Sources Reviewed):
1. **[Source Name]** - [What was learned]
2. **[Source Name]** - [What was learned]
...

### Key Findings for [Topic]:

**Core Structure:**
- [Key architectural elements]
- [Required components]
- [Optional enhancements]

**Critical Best Practices:**
- [Practice 1 with source]
- [Practice 2 with source]
- [Practice 3 with source]

**Latest Standards:**
- [Standard/version with source]
- [Recent changes]
- [Deprecated features to avoid]

**Common Patterns:**
- [Pattern 1]
- [Pattern 2]
- [Pattern 3]

---

## ✅ RESEARCH COMPLETE

**Total Information Gathered:**
- [Topic 1]: [Summary]
- [Topic 2]: [Summary]

**Ready for your command to create [artifact type]! 🚀**
```

#### Step 5: Stop and Wait
**CRITICAL:** After completing the research summary:
1. DO NOT proceed to create files or code
2. DO NOT start generating artifacts
3. WAIT for explicit user command to proceed
4. Ask if they want any clarifications about the research

## Research Quality Checklist

Before stopping and presenting research summary, verify:

- [ ] Read from at least 10 sources per major topic
- [ ] Included official/primary documentation
- [ ] Noted version numbers and dates
- [ ] Cross-referenced conflicting information
- [ ] Identified latest best practices
- [ ] Documented deprecated approaches to avoid
- [ ] Extracted canonical code examples
- [ ] Noted platform/browser compatibility
- [ ] Checked for security considerations
- [ ] Verified accessibility requirements
- [ ] Listed required vs optional features
- [ ] Organized findings in clear structure
- [ ] Created actionable summary
- [ ] Stopped without creating artifacts

## Example Research Topics

### Technical Documentation Research
When researching for creating documentation:
- Read official specs (W3C, WHATWG, etc.)
- Review MDN for browser features
- Check web.dev for modern practices
- Read official framework docs
- Review popular implementations (GitHub)
- Check accessibility guidelines (WCAG)
- Review performance best practices
- Read security considerations

### Framework/Library Research
When researching a framework or library:
- Official documentation
- GitHub repository (README, issues, discussions)
- Release notes and changelogs
- Official examples and tutorials
- Community best practices (awesome-* lists)
- Stack Overflow common patterns
- Performance benchmarks
- Migration guides
- Compatibility matrices

### Standards/Specifications Research
When researching technical standards:
- Official specification document
- Reference implementations
- Validation tools
- Adoption status
- Browser/platform support
- Version history
- Related specifications
- Community feedback
- Implementation notes
- Migration paths from older versions

## Common Research Mistakes to Avoid

**DON'T:**
- Stop after reading just 2-3 sources
- Skip official documentation
- Rely only on blog posts or tutorials
- Use outdated sources (2+ years old)
- Ignore version-specific details
- Skip cross-referencing
- Start creating before research is complete
- Assume knowledge is current without verification

**DO:**
- Read broadly across source types
- Prioritize official documentation
- Note publication dates
- Cross-reference information
- Document sources clearly
- Wait for user confirmation
- Present findings before creating
- Ask clarifying questions

## Token Management

When conducting comprehensive research:
- Use browser tools efficiently (navigate to key sections)
- Read table of contents and targeted sections
- Capture code examples verbatim
- Note URLs for later reference
- Summarize long sections concisely
- Focus on latest stable versions
- Skip deprecated content unless specifically requested

## After Research - Before Creation

Once research is complete and summarized:

1. **Present Summary** - Show what was learned
2. **Highlight Key Findings** - Call out critical information
3. **Note Conflicts** - If sources disagree, mention it
4. **State Readiness** - Confirm ready to create
5. **Wait for Command** - Don't proceed until user says so
6. **Offer Clarifications** - Ask if they want more detail on anything

## Creating After Research

When user gives the command to create (e.g., "create the HTML files"):
1. Apply all best practices from research
2. Use latest standards documented
3. Include proper citations/comments where helpful
4. Follow patterns from canonical examples
5. Implement accessibility requirements
6. Use modern features appropriately
7. Avoid deprecated approaches
8. Structure according to latest conventions

## Example Workflow

**User:** "Do research on creating modern HTML documentation systems, then create the files"

**Agent Response:**
```
I'll conduct comprehensive research on modern HTML documentation 
practices by reading from 10+ authoritative sources, then stop 
and wait for your command to create the files.

[Conducts research from MDN, web.dev, W3C, etc.]

## RESEARCH SUMMARY - READY FOR CREATION

### Modern HTML Research (12 Sources Reviewed):
1. **MDN Web Docs** - HTML5 semantic elements, accessibility
2. **web.dev Learn** - Modern practices, performance
3. **W3C HTML Spec** - Latest standard compliance
...

[Detailed findings]

✅ RESEARCH COMPLETE

Ready for your command to create the HTML documentation system! 🚀
```

**User:** "Create the files"

**Agent:** [Proceeds to create files using researched best practices]

---

## Summary

This workflow ensures that when you ask for research:
1. I read from **10+ authoritative sources**
2. I gather **comprehensive context** on the topic
3. I **synthesize findings** into actionable summary
4. I **stop and wait** for your command to proceed
5. I create artifacts using **latest best practices**

This results in high-quality, standards-compliant, modern implementations based on thorough research rather than assumptions or outdated knowledge.
