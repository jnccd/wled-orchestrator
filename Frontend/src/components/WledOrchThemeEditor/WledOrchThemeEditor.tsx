import { useQuery } from "@tanstack/react-query";
import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { Box, SimpleGrid } from "@chakra-ui/react";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import ThemePicker from "./ThemePicker";
import ThemePropertiesEditor from "./ThemePropertiesEditor";
import { usePageWidthThreshold } from "../../hooks/usePageWidthThreshold";
import ThemeModifiersEditor from "./ThemeModifiersEditor";
import ThemePreview from "./ThemePreview";

const WledOrchThemeEditor = () => {
  const gridPageWidthThreshold = usePageWidthThreshold(780);

  // React Query setup
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id === selectedGroupStore.selectedGroup
  )[0];

  return (
    <Box paddingTop={2}>
      <ThemePicker></ThemePicker>
      {!selectedGroup?.theme ? (
        <></>
      ) : (
        <SimpleGrid
          columns={gridPageWidthThreshold ? 3 : 1}
          justifyItems="center"
          gap={8}
          padding={6}
        >
          <ThemePropertiesEditor></ThemePropertiesEditor>
          <ThemeModifiersEditor></ThemeModifiersEditor>
          <ThemePreview></ThemePreview>
        </SimpleGrid>
      )}
    </Box>
  );
};

export default WledOrchThemeEditor;
