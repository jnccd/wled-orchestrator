import { Box, HStack, Text, useColorMode } from "@chakra-ui/react";
import ColorModeSwitch from "./ColorModeSwitch";

const NavBar = () => {
  const height = "100px";
  const navbarId = "navbar-container";

  const { colorMode } = useColorMode();

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
        paddingX={12}
        paddingY={6}
        backdropFilter={"auto"}
        backdropBlur={"5px"}
        backgroundColor={
          colorMode == "light" ? "rgba(255,255,255,.8)" : "rgba(24, 30, 41,.8)"
        }
      >
        <Text fontSize={20}>Wled Orchestrator</Text>
        <ColorModeSwitch></ColorModeSwitch>
      </HStack>
      <Box minHeight={height}></Box>
    </>
  );
};

export default NavBar;
