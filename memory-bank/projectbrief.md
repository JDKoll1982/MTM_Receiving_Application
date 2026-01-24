# Project Brief

Last Updated: 2026-01-24

## Project
MTM Receiving Application

## Purpose
A Windows desktop application for manufacturing receiving operations, focused on streamlined label generation, workflow management, and ERP (Infor Visual) integration.

## Core Requirements
- WinUI 3 UI with MVVM architecture (CommunityToolkit.Mvvm)
- Strict layer separation: View → ViewModel → Service → DAO → Database
- MySQL access via stored procedures only
- SQL Server / Infor Visual access is read-only
- DAOs return `Model_Dao_Result` and do not throw
- XAML must use `{x:Bind}` (compile-time binding)

## Current Documentation Work
- Agent Skills HTML guide under `docs/agent-skills-guide/`
- Goal: Provide a complete example skill and supporting instructions for creating skills
