import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getThemeTypes,
  getWledOrchState,
  setGroupTheme,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";
import {
  Button,
  Text,
  HStack,
  Menu,
  MenuButton,
  MenuItem,
  MenuList,
  Box,
} from "@chakra-ui/react";
import { ChevronDownIcon } from "@chakra-ui/icons";
import useSelectedGroupStore from "../../hooks/useLocalStore";

const WledOrchThemeEditor = () => {
  const selectedGroupStore = useSelectedGroupStore();

  // React Query setup
  const queryClient = useQueryClient();
  const themeTypesQuery = useQuery({
    queryKey: [],
    queryFn: getThemeTypes,
  });
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const changeThemeMutation = useMutation({
    mutationFn: setGroupTheme,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id == selectedGroupStore.selectedGroup
  )[0];

  return (
    <>
      <HStack justifyContent={"center"}>
        <Text as="b">The theme of group '{selectedGroup?.name}' is: </Text>
        <Menu>
          <MenuButton as={Button} rightIcon={<ChevronDownIcon />}>
            {selectedGroup?.theme?.typeName}
          </MenuButton>
          <MenuList>
            {themeTypesQuery.data?.themes?.map((x) => (
              <MenuItem
                key={x.name}
                onClick={(_) =>
                  changeThemeMutation.mutate({
                    groupId: selectedGroup?.id ?? "",
                    newTheme: {
                      $type: x.name,
                    },
                  })
                }
              >
                {x.name}
              </MenuItem>
            ))}
          </MenuList>
        </Menu>
      </HStack>
      <Box display="flex" flexDirection="column">
        {themeTypesQuery.data?.themes
          ?.filter((x) => x.name === selectedGroup?.theme?.typeName)[0]
          .properties?.map((x) => (
            <HStack justifyContent={"center"}>
              <Text>{x.name}:</Text>
              <Text>{x.type} input field</Text>
            </HStack>
          ))}
      </Box>
    </>
  );
};

export default WledOrchThemeEditor;
