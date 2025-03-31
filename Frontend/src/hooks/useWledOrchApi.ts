import { components } from "../generated/api";
import { createApiClient } from "../generated/zodios";
import { ZodiosHooks } from "@zodios/react";

export type LedSegmentGroups = components["schemas"]["DataStoreRoot"];
export type LedSegmentGroup = components["schemas"]["LedSegmentGroup"];
export type LedSegment = components["schemas"]["LedSegment"];
export type LedTheme = components["schemas"]["LedTheme"];
export type LedThemeModifier = components["schemas"]["LedThemeModifier"];

export type LedThemeTypes = components["schemas"]["LedThemeTypes"];
export type LedThemeTypeInfo = components["schemas"]["TypeInfo"];
export type LedThemeTypePropertyInfo = components["schemas"]["TypePropertyInfo"];
export type GenerateFrontendFormData = components["schemas"]["GenerateFrontendFormData"];

export const baseUrl = import.meta.env.VITE_DEV_BACKEND_ADDRESS ? import.meta.env.VITE_DEV_BACKEND_ADDRESS : window.location.href;
export const zodiosClient = createApiClient(baseUrl);
export const zodiosHooks = new ZodiosHooks("wledOrchApi", zodiosClient);