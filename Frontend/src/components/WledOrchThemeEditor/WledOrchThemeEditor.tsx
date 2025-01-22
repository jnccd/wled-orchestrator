import { useQuery } from "@tanstack/react-query";
import {
  baseUrl,
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { Box, SimpleGrid, Heading } from "@chakra-ui/react";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import ThemePicker from "./ThemePicker";
import ThemePropertiesEditor from "./ThemePropertiesEditor";
import { usePageWidth } from "../../hooks/usePageWidth";
import { Image } from "@chakra-ui/react";

const WledOrchThemeEditor = () => {
  const pageWidth = usePageWidth();

  // React Query setup
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id == selectedGroupStore.selectedGroup
  )[0];

  const themePreviewUrl =
    baseUrl + `/state/group/${selectedGroup?.id}/theme-preview`;
  const {
    data: base64Image,
    isLoading,
    error,
  } = useQuery({
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

  return (
    <Box paddingTop={2}>
      <ThemePicker></ThemePicker>
      {!selectedGroup?.theme ? (
        <></>
      ) : (
        <SimpleGrid columns={pageWidth > 780 ? 2 : 1} gap={8} padding={6}>
          <Box display="flex" flexDirection="column">
            <ThemePropertiesEditor></ThemePropertiesEditor>
          </Box>
          <Box width={"300px"}>
            <Heading fontSize={30} padding={4}>
              Preview:
            </Heading>
            <Image
              borderRadius={"8px"}
              minHeight={300}
              src={base64Image ?? ""}
            ></Image>
          </Box>
        </SimpleGrid>
      )}
    </Box>
  );
};

export default WledOrchThemeEditor;
