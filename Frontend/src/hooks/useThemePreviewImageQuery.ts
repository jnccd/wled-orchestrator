import { useQuery } from "@tanstack/react-query";
import { apiClient, wledOrchStateQueryKey } from "./useWledOrchApi";

export const useThemePreviewImageQuery = (groupId: string) => {
  const themePreviewUrl = `/state/groups/${groupId}/theme-preview`;
  return useQuery({
    queryKey: [wledOrchStateQueryKey, themePreviewUrl],
    queryFn: async () => {
      const response = await apiClient.get<string>(themePreviewUrl, {
        headers: {
          Accept: "image/png",
        },
        // The endpoint answers with the raw PNG base64 body (no JSON), do not let axios JSON-parse it.
        responseType: "text",
      });
      const base64 = response.data;
      return base64.startsWith("data:image/png;base64,")
        ? base64
        : `data:image/png;base64,${base64}`;
    },
  });
};
