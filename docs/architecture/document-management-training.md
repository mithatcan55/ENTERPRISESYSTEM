# Document Management Training

## Purpose

This module creates a reusable, versioned document backbone for the platform.
It is intentionally generic so the same PDF or image can be linked from multiple modules later.

## Why document data is split

The model uses three tables:

- `ManagedDocuments`
- `ManagedDocumentVersions`
- `DocumentAssociations`

### ManagedDocuments

This is the logical document record.
Examples:

- Motor technical drawing
- Product certificate
- User manual

### ManagedDocumentVersions

This stores the physical file versions of the same logical document.
Why:

- a PDF can be revised later
- old versions should remain auditable
- one version must be the current version

### DocumentAssociations

This links documents to business entities in a generic way.
Why:

- the same document can be attached to a material
- later the same pattern can be used for maintenance, shipment or approval entities
- the document itself does not need to know business-specific tables

## Phase 1 capabilities

- create a document with initial version
- add a new version
- mark only one current version
- link a document to any owner entity
- unlink a document
- list and detail queries

## Important design choices

### Generic owner link

The association uses:

- `OwnerEntityName`
- `OwnerEntityId`
- `LinkType`

This is intentional.
It avoids coupling the document module to a specific business module too early.

### No binary storage in database

This phase stores metadata only:

- file name
- content type
- storage path
- file size
- checksum

Actual binary storage can live in:

- file system
- object storage
- blob storage

## How material will use this later

Material will not own the file itself.
Instead it will create links such as:

- `OwnerEntityName = MATERIAL`
- `OwnerEntityId = 123`
- `LinkType = MAIN_IMAGE`

or

- `LinkType = GALLERY_IMAGE`
- `LinkType = TECHNICAL_PDF`
- `LinkType = CERTIFICATE`

This makes later validation easier:

- max 5 documents
- max 5 images
- only 1 main image

Those rules belong in the material module, not in the generic document module.
