import { Box, HStack, Text, useColorMode } from "@chakra-ui/react";
import ColorModeSwitch from "./ColorModeSwitch";
import WledOrchActivationSwitch from "./WledOrchActivationSwitch";

const NavBar = () => {
  const height = "100px";
  const navbarId = "navbar-container";

  const { colorMode } = useColorMode();
  const color =
    colorMode == "light" ? "rgba(255,255,255,.8)" : "rgba(24, 30, 41,.8)";

  return (
    <>
      <HStack
        id={navbarId}
        position={"fixed"}
        justifyContent={"space-between"}
        minHeight={height}
        top={0}
        left={0}
        right={0}
        paddingX={20}
        paddingY={6}
        boxShadow={"0 7px 25px " + color}
        backdropFilter={"auto"}
        backdropBlur={"4px"}
        backgroundColor={color}
        zIndex={999}
      >
        <Text fontSize={20}>Wled Orchestrator</Text>
        <HStack gap={5}>
          <ColorModeSwitch></ColorModeSwitch>
          <WledOrchActivationSwitch></WledOrchActivationSwitch>
        </HStack>
      </HStack>
      <Box minHeight={height}></Box>
    </>
  );
};

export default NavBar;
