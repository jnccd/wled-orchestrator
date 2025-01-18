import { Box, HStack, Show, Text, useColorMode } from "@chakra-ui/react";
import ColorModeSwitch from "./ColorModeSwitch";
import WledOrchActivationSwitch from "./WledOrchActivationSwitch";
import { useMaxPageWidthStore } from "../hooks/useLocalStore";
import { usePageWidth } from "../hooks/usePageWidth";

const NavBar = () => {
  const height = "100px";
  const navbarId = "navbar-container";
  const maxPageWidthStore = useMaxPageWidthStore();
  const pageWidth = usePageWidth();

  const { colorMode } = useColorMode();
  const color =
    colorMode == "light" ? "rgba(255,255,255,.8)" : "rgba(24, 30, 41,.8)";

  return (
    <>
      <HStack
        id={navbarId}
        position={"fixed"}
        maxWidth={"100vw"}
        minHeight={height}
        top={0}
        left={0}
        right={0}
        paddingX={20}
        paddingY={6}
        justifyContent={"center"}
        boxShadow={"0 7px 25px " + color}
        backdropFilter={"auto"}
        backdropBlur={"4px"}
        backgroundColor={color}
        zIndex={999}
      >
        <HStack
          width={"100%"}
          maxWidth={maxPageWidthStore.maxPageWidth}
          justifyContent={"space-between"}
        >
          <Text fontSize={pageWidth > 440 ? 20 : 16}>Wled Orchestrator</Text>
          <HStack gap={5}>
            <ColorModeSwitch></ColorModeSwitch>
            <WledOrchActivationSwitch></WledOrchActivationSwitch>
          </HStack>
        </HStack>
      </HStack>
      <Box minHeight={height}></Box>
    </>
  );
};

export default NavBar;
