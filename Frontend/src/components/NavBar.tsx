import { Box, HStack, Text, useColorMode } from "@chakra-ui/react";
import ColorModeSwitch from "./ColorModeSwitch";
import useWledAddresses from "../hooks/useWledAddresses";
import { useEffect, useState } from "react";
import DraggableButton from "./DraggableButton";

const NavBar = () => {
  const height = "100px";
  const navbarId = "navbar-container";
  const serverButtonIdPrefix = "server-button";

  const { colorMode } = useColorMode();
  const { wledAddresses, hasData: addressesLoaded } = useWledAddresses();
  const [wledNames, setWledNames] = useState([""]);

  useEffect(() => {
    if (addressesLoaded) {
      setWledNames(
        wledAddresses.addresses.map((a) => a.split(".").slice(-1)[0])
      );
    }
  }, [wledAddresses]);

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
        <Text>Wled Orchestrator</Text>
        {addressesLoaded && (
          <Box flexDirection={"row"}>
            {wledNames.map((a) => (
              <DraggableButton
                buttonName={a}
                id={serverButtonIdPrefix + "-" + a}
              ></DraggableButton>
            ))}
          </Box>
        )}
        <ColorModeSwitch></ColorModeSwitch>
      </HStack>
      <Box minHeight={height}></Box>
    </>
  );
};

export default NavBar;
