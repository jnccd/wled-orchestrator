import { Box, Heading } from "@chakra-ui/react";
import { useQuery } from "@tanstack/react-query";
import { Image } from "@chakra-ui/react";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { useThemePreviewImageQuery } from "../../hooks/useThemePreviewImageQuery";
import ThemePaneHeader from "./ThemePaneHeader";

const ThemePreview = () => {
  // React Query setup
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id === selectedGroupStore.selectedGroup
  )[0];

  const { data: themePreviewImage } = useThemePreviewImageQuery(
    selectedGroup?.id ?? ""
  );

  return (
    <Box width={"300px"}>
      <ThemePaneHeader>Preview</ThemePaneHeader>
      <Image
        borderRadius={"8px"}
        minHeight={300}
        src={themePreviewImage ?? ""}
      ></Image>
    </Box>
  );
};

export default ThemePreview;
