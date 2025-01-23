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
import { usePageWidthThreshold } from "../../hooks/usePageWidthThreshold";
import { Image } from "@chakra-ui/react";
import { useThemePreviewImage } from "../../hooks/useThemePreviewImage";

const WledOrchThemeEditor = () => {
  const gridPageWidthThreshold = usePageWidthThreshold(780);

  // React Query setup
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id == selectedGroupStore.selectedGroup
  )[0];

  const { data: themePreviewImage } = useThemePreviewImage(
    selectedGroup?.id ?? ""
  );

  return (
    <Box paddingTop={2}>
      <ThemePicker></ThemePicker>
      {!selectedGroup?.theme ? (
        <></>
      ) : (
        <SimpleGrid
          columns={gridPageWidthThreshold ? 2 : 1}
          gap={8}
          padding={6}
        >
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
              src={themePreviewImage ?? ""}
            ></Image>
          </Box>
        </SimpleGrid>
      )}
    </Box>
  );
};

export default WledOrchThemeEditor;
