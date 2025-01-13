import { useQuery } from "@tanstack/react-query";
import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";
import { Text, Box, Center, VStack } from "@chakra-ui/react";
import useSelectedGroupStore from "../../hooks/useLocalStore";
import ThemePicker from "./ThemePicker";
import ThemePropertiesEditor from "./ThemePropertiesEditor";

const WledOrchThemeEditor = () => {
  // React Query setup
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id == selectedGroupStore.selectedGroup
  )[0];

  return (
    <>
      <ThemePicker></ThemePicker>
      <Box display="flex" flexDirection="column">
        {!selectedGroup?.theme ? (
          <Text>-</Text>
        ) : (
          <Center>
            <VStack>
              <ThemePropertiesEditor></ThemePropertiesEditor>
            </VStack>
          </Center>
        )}
      </Box>
    </>
  );
};

export default WledOrchThemeEditor;
