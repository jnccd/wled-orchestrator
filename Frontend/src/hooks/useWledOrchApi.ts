import { components } from "../types/api";
import axios from "axios";

export type LedSegmentGroups = components["schemas"]["DataStoreRoot"];
export type LedSegmentGroup = components["schemas"]["LedSegmentGroup"];
export type LedSegment = components["schemas"]["LedSegment"];
export type LedTheme = components["schemas"]["LedTheme"];
export type LedThemeModifier = components["schemas"]["LedThemeModifier"];

export type LedThemeTypes = components["schemas"]["LedThemeTypes"];
export type LedThemeTypeInfo = components["schemas"]["TypeInfo"];
export type LedThemeTypePropertyInfo = components["schemas"]["TypePropertyInfo"];

export const baseUrl = import.meta.env.VITE_DEV_BACKEND_ADDRESS ? import.meta.env.VITE_DEV_BACKEND_ADDRESS : window.location.href;
export const apiClient = axios.create({
    baseURL: baseUrl,
    withCredentials: false,
})

export const wledOrchStateQueryKey = "wledOrchState"
export const getWledOrchState = () => 
  apiClient
    .get<LedSegmentGroups>(`/state`)
    .then((res) => res.data);

export const moveSegment = (args: {segmentId: string, targetGroupId: string | null}) => 
  apiClient
    .put(`/state/segments/${args.segmentId}/move`, null, {
      params: {
        targetGroupId: args.targetGroupId,
      },
    })
    .then((res) => res.data);

export const renameSegment = (args: {segmentId: string, newName: string}) => 
  apiClient
    .put(`/state/segments/${args.segmentId}/rename`, null, {
      params: {
        newName: args.newName,
      },
    })
    .then((res) => res.data);

export const renameGroup = (args: {groupId: string, newName: string}) => 
  apiClient
    .put(`/state/groups/${args.groupId}/rename`, null, {
      params: {
        newName: args.newName,
      },
    })
    .then((res) => res.data);

export const deleteGroup = (args: {groupId: string}) => 
  apiClient
    .delete(`/state/groups/${args.groupId}`)
    .then((res) => res.data);

export const setGroupTheme = (args: {groupId: string, newTheme: any}) => 
  apiClient
    .put(`/state/groups/${args.groupId}/theme`, args.newTheme)
    .then((res) => res.data);

export const addThemeModifier = (args: {groupId: string, newModifier: any}) => 
  apiClient
    .post(`/state/groups/${args.groupId}/theme/modifiers`, args.newModifier)
    .then((res) => res.data);

export const setThemeModifier = (args: {groupId: string, modifierId: string, newModifier: any}) => 
  apiClient
    .put(`/state/groups/${args.groupId}/theme/modifiers/${args.modifierId}`, args.newModifier)
    .then((res) => res.data);
      
export const setActivated = (args: {newActivated: boolean}) => 
  apiClient
    .put(`/state/activated`, null, {
      params: {
        newACtivated: args.newActivated,
      },
    })
    .then((res) => res.data);

export const getThemeTypes = () => 
  apiClient
    .get<LedThemeTypes>(`/theme-types`)
    .then((res) => res.data);