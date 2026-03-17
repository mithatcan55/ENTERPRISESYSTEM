# Authorization Policy Training

## Purpose

This module extends the current permission and T-Code model with a dynamic field policy layer.
The goal is to make UI and API behavior configurable without code changes for common cases like:

- hide a field
- make a field read-only
- mask a field value
- disable filter visibility
- disable export visibility

## Why a second authorization layer exists

The current system already answers:

- can the user enter this page?
- can the user execute this action?

That is not enough for enterprise cases like:

- the user can open Material detail, but cannot see high-value price data
- the user can see a field on detail, but not on table
- the user can filter by a field, but cannot export it

This module answers those finer questions.

## Core tables

### AuthorizationFieldDefinitions

This table defines the available fields for a business entity.

Important columns:

- `EntityName`
- `FieldName`
- `DisplayName`
- `DataType`
- `AllowedSurfaces`
- `DefaultVisible`
- `DefaultEditable`
- `DefaultFilterable`
- `DefaultExportable`

Think of this table as metadata. It says which fields exist and what their default behavior is.

### AuthorizationFieldPolicies

This table stores the dynamic rules.

Important columns:

- `EntityName`
- `FieldName`
- `Surface`
- `TargetType`
- `TargetKey`
- `Effect`
- `ConditionFieldName`
- `ConditionOperator`
- `CompareValue`
- `MaskingMode`
- `Priority`

Example meaning:

- for `Material.Price`
- on `DETAIL`
- for `ANY` user
- if field value is `GT 10000`
- apply `MASK`

## Evaluation flow

The evaluator works in four steps:

1. load field definitions for the requested entity
2. load active policy rules for the requested surface
3. keep only the rules matching the current user target
4. apply matching rules in priority order

Result:

- `Visible`
- `Editable`
- `Filterable`
- `Exportable`
- `Masked`
- `MaskingMode`

## Supported target types in phase 1

- `ANY`
- `USER`
- `ROLE`
- `PERMISSION`

## Supported effects in phase 1

- `HIDE`
- `SHOW`
- `READONLY`
- `EDITABLE`
- `MASK`
- `SHOW_FILTER`
- `HIDE_FILTER`
- `SHOW_EXPORT`
- `HIDE_EXPORT`

## Supported operators in phase 1

- `ALWAYS`
- `EQ`
- `NE`
- `GT`
- `GTE`
- `LT`
- `LTE`
- `CONTAINS`
- `STARTS_WITH`
- `ENDS_WITH`
- `IS_NULL`
- `NOT_NULL`

## Example policy

Field definition:

- `EntityName = MATERIAL`
- `FieldName = PRICE`
- `DataType = DECIMAL`

Policy:

- `Surface = DETAIL`
- `TargetType = ANY`
- `Effect = MASK`
- `ConditionFieldName = PRICE`
- `ConditionOperator = GT`
- `CompareValue = 10000`
- `MaskingMode = FULL`

Meaning:

- all users will see the field masked when the price is greater than 10000

Another policy can override it for a special permission:

- `TargetType = PERMISSION`
- `TargetKey = MATERIAL.PRICE.HIGHVALUE.VIEW`
- `Effect = SHOW`
- `Priority = 100`

## Why priority exists

Enterprise policy sets can conflict.

Example:

- default rule masks price above 10000
- finance permission should override that mask

Priority gives us deterministic resolution. Higher-priority rules are applied later and can override lower-priority behavior.

## Why this is dynamic

This module is dynamic because:

- field definitions live in tables
- rules live in tables
- matching and effects are interpreted at runtime

That means a new field policy can be added without rebuilding the backend, as long as the field definition already exists.

## What this phase does not do yet

- no visual admin screen yet
- no full expression language
- no cross-entity joins in rules
- no field-group inheritance

Those can be added later without replacing the core model.
