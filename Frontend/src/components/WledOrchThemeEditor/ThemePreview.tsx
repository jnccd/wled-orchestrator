import { Box, Flex, Image } from "@chakra-ui/react";
import { useQuery } from "@tanstack/react-query";
import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import { useThemePreviewImageQuery } from "../../hooks/useThemePreviewImageQuery";
import { readProperty } from "../../utils/untypedPropertyAccess";
import { NAVBAR_HEIGHT } from "../NavBar";
import ThemePaneHeader from "./ThemePaneHeader";
import PreviewTimeAxis from "./PreviewTimeAxis";

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

  const previewType = readProperty(selectedGroup?.theme, "previewType");

  return (
    <Box
      width={"348px"}
      // Keep the preview in view while the (now long) properties and modifiers columns scroll: it
      // sticks just below the fixed navbar, and alignSelf keeps it at its own height so it can
      // actually slide down inside its grid row.
      alignSelf={"start"}
      position={"sticky"}
      top={`calc(${NAVBAR_HEIGHT} + 12px)`}
    >
      <ThemePaneHeader>Preview</ThemePaneHeader>
      <Flex alignItems="stretch">
        {/* The time axis of the preview is its y axis; the label strip stretches with the image. */}
        <PreviewTimeAxis previewType={previewType} />
        {themePreviewImage ? (
          <Image
            borderRadius={"8px"}
            display={"block"}
            width={"300px"}
            height={"auto"}
            src={themePreviewImage}
          ></Image>
        ) : (
          <Box width={"300px"} minHeight={300} />
        )}
      </Flex>
    </Box>
  );
};

export default ThemePreview;
