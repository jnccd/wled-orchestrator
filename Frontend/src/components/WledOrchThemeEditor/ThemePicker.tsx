import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getThemeTypes,
  getWledOrchState,
  setGroupTheme,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
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
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import { readProperty } from "../../utils/untypedPropertyAccess";

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
    (x) => x.id === selectedGroupStore.selectedGroup
  )[0];

  if (!wledOrchStateQuery.isSuccess) return <></>;

  const currentThemeType = selectedGroup?.theme
    ? readProperty(selectedGroup.theme, "$type")
    : undefined;
  // The groups last settings per theme type (kept server side), so picking a type again restores
  // them instead of resetting every parameter.
  const themeStash = readProperty(selectedGroup ?? {}, "themeStash") ?? {};

  const fontSize = "1.2rem";
  return (
    <HStack justifyContent={"center"} flexWrap={"wrap"}>
      <Text as="b" fontSize={fontSize}>
        The theme of group '{selectedGroup?.name}' is{" "}
      </Text>
      <Menu>
        <MenuButton
          as={Button}
          fontSize={fontSize}
          paddingY={6}
          rightIcon={<ChevronDownIcon />}
        >
          {selectedGroup?.theme?.typeName ?? "None"}
        </MenuButton>
        <MenuList>
          {/* "None" (= no theme) is a valid state, e.g. a freshly created group starts without one. */}
          <MenuItem
            onClick={() =>
              changeThemeMutation.mutate({
                groupId: selectedGroup?.id ?? "",
                newTheme: null,
              })
            }
          >
            None
          </MenuItem>
          {themeTypesQuery.data?.themes?.map((x) => {
            const stashedTheme = x.typeDiscriminator
              ? themeStash[x.typeDiscriminator]
              : undefined;
            const isCurrent = x.typeDiscriminator === currentThemeType;
            return (
              <MenuItem
                key={x.name}
                isDisabled={isCurrent}
                onClick={() =>
                  changeThemeMutation.mutate({
                    groupId: selectedGroup?.id ?? "",
                    // Restore this groups stashed settings for that type, only fall back to a fresh
                    // theme (server defaults) when it was never used.
                    newTheme: stashedTheme ?? { $type: x.typeDiscriminator },
                  })
                }
              >
                {x.name}
                {stashedTheme && !isCurrent ? " (saved)" : ""}
              </MenuItem>
            );
          })}
        </MenuList>
      </Menu>
      <Text as="b" fontSize={fontSize}>
        .
      </Text>
    </HStack>
  );
};

export default ThemePicker;
