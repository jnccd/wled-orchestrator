#!/bin/bash
cd "$(dirname "$0")" || exit

pnpx openapi-zod-client http://localhost:7778/swagger/v1/swagger.json -o ./Frontend/src/generated/zodios.ts
pnpx openapi-typescript@5.4.1 http://localhost:7778/swagger/v1/swagger.json --output ./Frontend/src/generated/api.ts