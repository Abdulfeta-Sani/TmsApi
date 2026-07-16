# API Versioning Policy

## Purpose

This document defines how the Training Management System (TMS) API is versioned and how changes are introduced without disrupting existing clients.

## Breaking Changes

A new API version is required when any of the following changes are made:

- Removing an existing response field.
- Renaming an existing field.
- Changing the meaning or data type of a field.
- Changing an HTTP status code returned by an endpoint.
- Tightening validation rules that make previously valid requests fail.
- Changing the default sorting or paging behavior.
- Removing an endpoint or changing its route.

These changes can cause existing client applications to fail and therefore require a new API version.

## Non-Breaking (Additive) Changes

The following changes are considered backward compatible and may be introduced without creating a new API version:

- Adding a new optional response field.
- Adding a new endpoint.
- Adding an optional query parameter.
- Improving performance without changing the API contract.
- Adding new documentation or metadata.

Existing clients continue to function without modification.

## Deprecation and Sunset Policy

When a new major API version is released, the previous version enters a deprecation period.

The TMS API guarantees that a deprecated version will remain available for **at least six months** after the new version is released. This allows partner organizations, including training centers with scheduled maintenance windows, sufficient time to migrate.

After the sunset date, the deprecated version may be removed.

## Client Communication

Clients are informed of API deprecation through multiple channels:

- `Deprecation` response header.
- `Sunset` response header.
- `Link` response header pointing to the successor version.
- CHANGELOG updates.
- Email notifications to registered API consumers.
- Calendar reminders before the shutdown date.

This ensures clients receive both machine-readable and human-readable migration guidance.

## Version Migration

Clients may migrate directly between supported versions.

For example, a client using **V1** may upgrade directly to **V3** without first adopting **V2**.

Clients are never required to upgrade through every intermediate version.
