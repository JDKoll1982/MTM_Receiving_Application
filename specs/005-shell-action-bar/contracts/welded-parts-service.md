# Welded Parts Service Contract

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md) | **Data model**: [data-model.md](../data-model.md)

This is the surface the Welded Part Numbers page codes against, and the whole of what US8 adds. The page
knows one interface, the interface knows one DAO, and the DAO knows three stored procedures that already
exist. Nothing here creates a table or a procedure: the list is the `settings_weldedcoils` list Extra Tools,
Welded Coils already maintains, so both pages show one set of rows (FR-037).

## IService_Settings_ReceivingWeldedParts

```csharp
public interface IService_Settings_ReceivingWeldedParts
{
    Task<Model_Dao_Result<List<Model_Settings_ReceivingWeldedPart>>> GetAllAsync();
    Task<Model_Dao_Result<int>> AddAsync(string partId);
    Task<Model_Dao_Result> DeleteAsync(int id);
}
```

| Member | Behaviour |
| --- | --- |
| `GetAllAsync` | every stored row, in part order. The page sorts and filters for display; the service does not |
| `AddAsync` | normalises the part number, applies the column limit, confirms the part in the part master, then inserts. Returns the new row id |
| `DeleteAsync` | removes one row by id |

Expected failures are returned as `Model_Dao_Result` values. The service never throws for a refused part, so
the page can show the message it carries.

## Rules the service owns

These are the only place the rules live, which is what makes them testable without a page.

| Rule | Outcome |
| --- | --- |
| Trim, then upper-case the part number | stored form is canonical, so two rows cannot differ only by case or padding |
| Blank after trimming | refused, nothing stored |
| Longer than 11 characters | refused, nothing stored. 11 is the `settings_weldedcoils.partid` width |
| Part missing from the part master | refused, nothing stored |
| Part master cannot be reached | refused with a message, nothing stored, because storing a part the ERP does not recognise is worse than refusing it |
| Part already listed | refused by the page before the service is called, so the operator sees which part is already there |

The part master is Infor Visual and stays read only. The only call made against it is
`IService_InforVisual.PartExistsAsync`.

## Dao_Settings_ReceivingWeldedParts

Instance DAO with the connection string injected, returning `Model_Dao_Result` values for expected failures.

| Member | Stored procedure | Parameters |
| --- | --- | --- |
| `GetAllAsync` | `sp_Settings_WeldedCoils_GetAll` | none |
| `InsertAsync` | `sp_Settings_WeldedCoils_Insert` | `@p_partid` in, `@p_new_id` out |
| `DeleteAsync` | `sp_Settings_WeldedCoils_Delete` | `p_id` in |

No SQL text appears in C#, and no other write path to the table is opened.

## Model_Settings_ReceivingWeldedPart

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | `settings_weldedcoils.id` |
| `PartId` | `string` | upper-case part number, at most 11 characters |

Rows are added and removed, never edited in place (FR-039).

## What the page must not do

- It must not read or write the welded list any way other than through this interface.
- It must not keep its own cache of the list that could disagree with Extra Tools after a change of its own.
- It must not treat a refused part as a success, or leave a half-added row behind.
