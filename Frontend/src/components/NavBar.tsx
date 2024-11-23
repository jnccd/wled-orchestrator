import {
  Box,
  Button,
  Card,
  Heading,
  HStack,
  Text,
  useColorMode,
} from "@chakra-ui/react";
import ColorModeSwitch from "./ColorModeSwitch";
import SearchInput from "./SearchInput";
import useWledAddresses from "../hooks/useWledAddresses";

interface Props {
  onSearch: (searchText: string) => void;
}

const NavBar = ({ onSearch }: Props) => {
  const height = "100px";
  const navbarId = "navbar-container";
  const { colorMode } = useColorMode();
  const { wledAddresses, error, isLoading, apiClient } = useWledAddresses();

  return (
    <HStack
      id={navbarId}
      position={"fixed"}
      justifyContent={"space-between"}
      padding={8}
      paddingX={12}
      minHeight={height}
      top={0}
      left={0}
      right={0}
      paddingY={6}
      bg={"none"}
      backdropFilter={"auto"}
      backdropBlur={"5px"}
      backgroundColor={
        colorMode == "light" ? "rgba(255,255,255,.8)" : "rgba(24, 30, 41,.8)"
      }
      transition="all 0.5s"
      zIndex={99999}
    >
      <Text>Dev</Text>
      <Box justifyItems={"flex-start"}>
        <Card
          margin={0}
          display={"inline-block"}
          borderRadius={"var(--card-radius) var(--card-radius) 0px 0px"}
        >
          <Text padding={2} textAlign={"start"} fontSize={"small"}>
            TestHeader
          </Text>
        </Card>
        <Card
          borderRadius={
            "0px var(--card-radius) var(--card-radius) var(--card-radius)"
          }
        >
          <Box flexDirection={"row"}>
            {wledAddresses.addresses.map((a) => (
              <Button
                className="wledServerButton"
                transition="all 0.5s"
                margin={2}
                size="lg"
              >
                {a.split(".").slice(-1)[0]}
              </Button>
            ))}
          </Box>
        </Card>
      </Box>
      <ColorModeSwitch></ColorModeSwitch>
    </HStack>
  );
};

export default NavBar;
