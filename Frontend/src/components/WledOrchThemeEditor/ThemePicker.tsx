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
} from "@chakra-ui/react";
import { ChevronDownIcon } from "@chakra-ui/icons";
import useSelectedGroupStore from "../../hooks/useLocalStore";

const ThemePicker = () => {
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

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id == selectedGroupStore.selectedGroup
  )[0];

  return (
    <HStack justifyContent={"center"}>
      <Text as="b">The theme of group '{selectedGroup?.name}' is </Text>
      <Menu>
        <MenuButton as={Button} rightIcon={<ChevronDownIcon />}>
          {selectedGroup?.theme?.typeName ?? "None"}
        </MenuButton>
        <MenuList>
          {themeTypesQuery.data?.themes?.map((x) => (
            <MenuItem
              key={x.name}
              onClick={(_) =>
                changeThemeMutation.mutate({
                  groupId: selectedGroup?.id ?? "",
                  newTheme: {
                    $type: x.typeDiscriminator,
                  },
                })
              }
            >
              {x.name}
            </MenuItem>
          ))}
        </MenuList>
      </Menu>
      <Text as="b">.</Text>
    </HStack>
  );
};

export default ThemePicker;
