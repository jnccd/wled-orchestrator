import apiClient from "../services/api-client";
import { components } from "../types/api";

export type LedSegmentGroups = components["schemas"]["DataStoreRoot"];
export type LedSegmentGroup = components["schemas"]["LedSegmentGroup"];
export type LedSegment = components["schemas"]["LedSegment"];

export const wledOrchStateQueryKey = "wledOrchState"

export const getWledOrchState = () => 
  apiClient
    .get<LedSegmentGroups>("/state")
    .then((res) => res.data);

export const moveSegment = (args: {segmentId: string, targetGroupId: string | null}) => 
  apiClient
    .put("/state/segment/move", null, {
      params: {
        segmentId: args.segmentId,
        targetGroupId: args.targetGroupId,
      },
    })
    .then((res) => res.data);

export const renameSegment = (args: {segmentId: string, newName: string}) => 
  apiClient
    .put("/state/segment/rename", null, {
      params: {
        segmentId: args.segmentId,
        newName: args.newName,
      },
    })
    .then((res) => res.data);

export const renameGroup = (args: {groupId: string, newName: string}) => 
  apiClient
    .put("/state/group/rename", null, {
      params: {
        groupId: args.groupId,
        newName: args.newName,
      },
    })
    .then((res) => res.data);
      