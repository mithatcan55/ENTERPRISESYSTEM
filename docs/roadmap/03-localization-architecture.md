# Localization Architecture (TR / DE / EN)

## Objective
Provide deterministic multi-language behavior for API validation and error responses.
Supported cultures:
- tr-TR (default)
- de-DE
- en-US

## Resolution Order
1. `x-culture` request header (optional override for controlled clients)
2. `Accept-Language` header
3. User profile culture (if authenticated and configured)
4. System default (`tr-TR`)

## What Must Be Localized
1. Validation messages
2. Application exception messages
3. ProblemDetails `title` and `detail`
4. Operational user-facing messages (not internal technical logs)

## What Must Stay Stable Across Languages
1. `errorCode`
2. HTTP status code
3. Correlation id
4. Contract field names

## Resource Strategy
- Use resource keys per bounded capability, for example:
  - `Errors.Auth.InvalidCredentials`
  - `Errors.User.NotFound`
  - `Validation.User.EmailRequired`
- Keep key names in English and values translated.
- Add a missing-key monitor log (`Warning` level).

## API Error Contract Guidance
Always return:
- `type`
- `title` (localized)
- `status`
- `detail` (localized)
- `errorCode` (stable)
- `correlationId`

## Testing Matrix
For each critical endpoint, verify:
1. TR response
2. DE response
3. EN response
4. Fallback behavior for unsupported culture

## Example Rollout Order
1. Implement shared localization service + resource catalogs.
2. Integrate into global exception handler.
3. Integrate into validation response factory.
4. Cover auth and user-management flows first.