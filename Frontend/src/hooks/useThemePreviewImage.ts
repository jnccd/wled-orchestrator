import { useQuery } from "@tanstack/react-query";
import { baseUrl, wledOrchStateQueryKey } from "./useWledOrchApi";

export const useThemePreviewImage = (groupId: string) => {
    const themePreviewUrl =
        baseUrl + `/state/group/${groupId}/theme-preview`;
    return useQuery({
        queryKey: [wledOrchStateQueryKey, themePreviewUrl],
        queryFn: async () => {
        const response = await fetch(themePreviewUrl, {
            headers: {
            Accept: "image/png",
            },
        });
        return await response.text();
        },
    });
}