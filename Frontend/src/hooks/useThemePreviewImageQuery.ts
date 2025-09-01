import { useQuery } from "@tanstack/react-query";
import { apiClient, baseUrl, wledOrchStateQueryKey } from "./useWledOrchApi";

export const useThemePreviewImageQuery = (groupId: string) => {
  const themePreviewUrl = baseUrl + `/state/groups/${groupId}/theme-preview`;
  return useQuery({
    queryKey: [wledOrchStateQueryKey, themePreviewUrl],
    queryFn: async () => {
      const response = await apiClient.get(themePreviewUrl, {
        headers: {
          Accept: "image/png",
        },
      });
      return await response.data;
    },
  });
};
