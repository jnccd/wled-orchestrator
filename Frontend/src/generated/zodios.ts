import { makeApi, Zodios, type ZodiosOptions } from "@zodios/core";
import { z } from "zod";

const LedThemeModifier = z
  .object({
    id: z.string().uuid(),
    typeName: z.string().nullable(),
    enabled: z.boolean(),
  })
  .partial();
const RotateColorsModifier = LedThemeModifier;
const WakeupModifier = LedThemeModifier;
const LedTheme = z
  .object({
    id: z.string().uuid(),
    typeName: z.string().nullable(),
    modifiers: z
      .array(z.union([LedThemeModifier, RotateColorsModifier, WakeupModifier]))
      .nullable(),
  })
  .partial();
const LedThemeDaylight = LedTheme;
const LedThemeSingleColor = LedTheme;
const putStategroupsGroupIdtheme_Body = z.union([
  LedTheme,
  LedThemeDaylight,
  LedThemeSingleColor,
]);
const postStategroupsGroupIdthememodifiers_Body = z.union([
  LedThemeModifier,
  RotateColorsModifier,
  WakeupModifier,
]);
const LedSegment = z
  .object({
    wledServerAddress: z.string().nullable(),
    segmentIndex: z.number().int(),
    name: z.string().nullable(),
    start: z.number().int(),
    length: z.number().int(),
    id: z.string().nullable(),
  })
  .partial();
const ColorRgb = z
  .object({ r: z.number().int(), g: z.number().int(), b: z.number().int() })
  .partial();
const LedSegmentGroup = z
  .object({
    id: z.string().uuid(),
    name: z.string().nullable(),
    ledSegments: z.array(LedSegment).nullable(),
    theme: z
      .union([LedTheme, LedThemeDaylight, LedThemeSingleColor])
      .nullable(),
    displayColor: ColorRgb,
    isEdited: z.boolean(),
  })
  .partial();
const DataStoreRoot = z
  .object({
    activated: z.boolean(),
    groups: z.array(LedSegmentGroup).nullable(),
  })
  .partial();
const GenerateFrontendFormData = z
  .object({
    inputType: z.string().nullable(),
    minValue: z.number(),
    maxValue: z.number(),
  })
  .partial();
const TypePropertyInfo = z
  .object({
    name: z.string().nullable(),
    displayName: z.string().nullable(),
    type: z.string().nullable(),
    settings: GenerateFrontendFormData,
  })
  .partial();
const TypeInfo = z
  .object({
    name: z.string().nullable(),
    typeDiscriminator: z.string().nullable(),
    properties: z.array(TypePropertyInfo).nullable(),
  })
  .partial();
const LedThemeTypes = z
  .object({
    themes: z.array(TypeInfo).nullable(),
    modifiers: z.array(TypeInfo).nullable(),
  })
  .partial();

export const schemas = {
  LedThemeModifier,
  RotateColorsModifier,
  WakeupModifier,
  LedTheme,
  LedThemeDaylight,
  LedThemeSingleColor,
  putStategroupsGroupIdtheme_Body,
  postStategroupsGroupIdthememodifiers_Body,
  LedSegment,
  ColorRgb,
  LedSegmentGroup,
  DataStoreRoot,
  GenerateFrontendFormData,
  TypePropertyInfo,
  TypeInfo,
  LedThemeTypes,
};

const endpoints = makeApi([
  {
    method: "get",
    path: "/state",
    alias: "getState",
    requestFormat: "json",
    response: DataStoreRoot,
  },
  {
    method: "put",
    path: "/state/activated",
    alias: "putStateactivated",
    requestFormat: "json",
    parameters: [
      {
        name: "newActivated",
        type: "Query",
        schema: z.boolean(),
      },
    ],
    response: z.void(),
  },
  {
    method: "delete",
    path: "/state/groups/:groupId",
    alias: "deleteStategroupsGroupId",
    requestFormat: "json",
    parameters: [
      {
        name: "groupId",
        type: "Path",
        schema: z.string(),
      },
    ],
    response: z.void(),
  },
  {
    method: "put",
    path: "/state/groups/:groupId/name",
    alias: "putStategroupsGroupIdname",
    requestFormat: "json",
    parameters: [
      {
        name: "groupId",
        type: "Path",
        schema: z.string(),
      },
      {
        name: "newName",
        type: "Query",
        schema: z.string(),
      },
    ],
    response: z.void(),
  },
  {
    method: "put",
    path: "/state/groups/:groupId/theme",
    alias: "putStategroupsGroupIdtheme",
    requestFormat: "json",
    parameters: [
      {
        name: "body",
        type: "Body",
        schema: putStategroupsGroupIdtheme_Body,
      },
      {
        name: "groupId",
        type: "Path",
        schema: z.string(),
      },
    ],
    response: z.void(),
  },
  {
    method: "get",
    path: "/state/groups/:groupId/theme-preview",
    alias: "getStategroupsGroupIdthemePreview",
    requestFormat: "json",
    parameters: [
      {
        name: "groupId",
        type: "Path",
        schema: z.string(),
      },
    ],
    response: z.void(),
  },
  {
    method: "post",
    path: "/state/groups/:groupId/theme/modifiers",
    alias: "postStategroupsGroupIdthememodifiers",
    requestFormat: "json",
    parameters: [
      {
        name: "body",
        type: "Body",
        schema: postStategroupsGroupIdthememodifiers_Body,
      },
      {
        name: "groupId",
        type: "Path",
        schema: z.string(),
      },
      {
        name: "index",
        type: "Query",
        schema: z.number().int().optional(),
      },
    ],
    response: z.void(),
  },
  {
    method: "put",
    path: "/state/groups/:groupId/theme/modifiers/:modifierId",
    alias: "putStategroupsGroupIdthememodifiersModifierId",
    requestFormat: "json",
    parameters: [
      {
        name: "body",
        type: "Body",
        schema: postStategroupsGroupIdthememodifiers_Body,
      },
      {
        name: "groupId",
        type: "Path",
        schema: z.string(),
      },
      {
        name: "modifierId",
        type: "Path",
        schema: z.string(),
      },
    ],
    response: z.void(),
  },
  {
    method: "delete",
    path: "/state/groups/:groupId/theme/modifiers/:modifierId",
    alias: "deleteStategroupsGroupIdthememodifiersModifierId",
    requestFormat: "json",
    parameters: [
      {
        name: "groupId",
        type: "Path",
        schema: z.string(),
      },
      {
        name: "modifierId",
        type: "Path",
        schema: z.string(),
      },
    ],
    response: z.void(),
  },
  {
    method: "put",
    path: "/state/segments/:segmentId/move",
    alias: "putStatesegmentsSegmentIdmove",
    requestFormat: "json",
    parameters: [
      {
        name: "segmentId",
        type: "Path",
        schema: z.string(),
      },
      {
        name: "targetGroupId",
        type: "Query",
        schema: z.string().optional(),
      },
    ],
    response: z.void(),
  },
  {
    method: "put",
    path: "/state/segments/:segmentId/name",
    alias: "putStatesegmentsSegmentIdname",
    requestFormat: "json",
    parameters: [
      {
        name: "segmentId",
        type: "Path",
        schema: z.string(),
      },
      {
        name: "newName",
        type: "Query",
        schema: z.string(),
      },
    ],
    response: z.void(),
  },
  {
    method: "get",
    path: "/theme-types",
    alias: "getThemeTypes",
    requestFormat: "json",
    response: LedThemeTypes,
  },
]);

export const api = new Zodios(endpoints);

export function createApiClient(baseUrl: string, options?: ZodiosOptions) {
  return new Zodios(baseUrl, endpoints, options);
}
