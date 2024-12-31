import { components } from "../types/api";
import axios from "axios";

export type LedSegmentGroups = components["schemas"]["DataStoreRoot"];
export type LedSegmentGroup = components["schemas"]["LedSegmentGroup"];
export type LedSegment = components["schemas"]["LedSegment"];

const apiClient = axios.create({
    baseURL: import.meta.env.VITE_DEV_BACKEND_ADDRESS ? import.meta.env.VITE_DEV_BACKEND_ADDRESS : window.location.href,
    withCredentials: false,
})

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
      
export const setActivated = (args: {newActivated: boolean}) => 
  apiClient
    .put("/state/activated", null, {
      params: {
        newACtivated: args.newActivated,
      },
    })
    .then((res) => res.data);