import apiClient from "../services/api-client";
import { components } from "../types/api";

export type LedSegmentGroups = components["schemas"]["DataStoreRoot"];
export type LedSegmentGroup = components["schemas"]["LedSegmentGroup"];
export type LedSegment = components["schemas"]["LedSegment"];

export const getWledOrchState = () => 
  apiClient
    .get<LedSegmentGroups>("/state")
    .then((res) => res.data);

export const putWledOrchState = (data: LedSegmentGroups) => 
  apiClient
    .put<LedSegmentGroups>("/state", data)
    .then((res) => res.data);

export const moveSegment = (args: {segmentId: string, targetGroupId: string | null}) => 
    apiClient
      .put("/state/moveSegment", null, {
        params: {
          segmentId: args.segmentId,
          targetGroupId: args.targetGroupId,
        },
      })
      .then((res) => res.data);

export const wledOrchStateQueryKey = "wledOrchState"
