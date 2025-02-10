import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addThemeModifier,
  getThemeTypes,
  getWledOrchState,
  setThemeModifier,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import {
  Text,
  Heading,
  Box,
  Menu,
  MenuButton,
  MenuList,
  MenuItem,
  Button,
  HStack,
} from "@chakra-ui/react";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import { ChevronDownIcon } from "@chakra-ui/icons";
import Draggable from "../Draggable";
import { readProperty } from "../../utils/untypedPropertyAccess";
import ColorEditor from "../GenericEditors/ColorEditor";
import DoubleEditor from "../GenericEditors/DoubleEditor";

const firstCharToLowerCase = (
  text: string | null | undefined
): string | undefined =>
  !text || text === "" ? undefined : text[0]?.toLowerCase() + text.slice(1);

const ThemeModifiersEditor = () => {
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
  const addModifierMutation = useMutation({
    mutationFn: addThemeModifier,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });
  const changeModifierMutation = useMutation({
    mutationFn: setThemeModifier,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const selectedGroupStore = useSelectedGroupStore();
  const selectedGroup = wledOrchStateQuery.data?.groups?.filter(
    (x) => x.id === selectedGroupStore.selectedGroup
  )[0];

  return (
    <Box display="flex" flexDirection="column" alignItems="center">
      <Heading padding={4} fontSize={30}>
        Theme Modifiers:
      </Heading>
      <Menu>
        <MenuButton
          as={Button}
          rightIcon={<ChevronDownIcon />}
          marginBottom={5}
        >
          Add Modifier
        </MenuButton>
        <MenuList>
          {themeTypesQuery.data?.modifiers?.map((x) => (
            <MenuItem
              key={x.name}
              onClick={(_) =>
                addModifierMutation.mutate({
                  groupId: selectedGroup?.id ?? "",
                  newModifier: { $type: x.typeDiscriminator },
                })
              }
            >
              {x.name}
            </MenuItem>
          ))}
        </MenuList>
      </Menu>
      {selectedGroup?.theme?.modifiers?.map((modifier) => {
        return (
          <Draggable
            id={modifier.id ?? ""}
            className={`modifier${modifier.id}`}
            children={
              <>
                <Text>{modifier.typeName}</Text>
                {themeTypesQuery.data?.modifiers
                  ?.filter(
                    (x) =>
                      x.typeDiscriminator === readProperty(modifier, "$type")
                  )[0]
                  .properties?.map((modifierProperty) => {
                    const propertyName = firstCharToLowerCase(
                      modifierProperty.name
                    );
                    if (!propertyName) {
                      return <Text>Invalid property name!</Text>;
                    }

                    let themePropertyUi: JSX.Element = <></>;
                    if (modifierProperty.type === "ColorHsv") {
                      themePropertyUi = (
                        <ColorEditor
                          editingObject={selectedGroup?.theme ?? {}}
                          propertyName={propertyName}
                          onChange={(object: any) =>
                            changeModifierMutation.mutate({
                              groupId: selectedGroup?.id ?? "",
                              modifierId: modifier.id ?? "",
                              newModifier: object,
                            })
                          }
                        ></ColorEditor>
                      );
                    } else if (modifierProperty.type === "Double") {
                      themePropertyUi = (
                        <DoubleEditor
                          editingObject={selectedGroup?.theme ?? {}}
                          propertyName={propertyName}
                          onChange={(object: any) =>
                            changeModifierMutation.mutate({
                              groupId: selectedGroup?.id ?? "",
                              modifierId: modifier.id ?? "",
                              newModifier: object,
                            })
                          }
                        ></DoubleEditor>
                      );
                    } else {
                      themePropertyUi = (
                        <Text>
                          {modifierProperty.type} input field is undefined!
                        </Text>
                      );
                    }

                    return (
                      <HStack
                        width={"100%"}
                        key={propertyName}
                        padding={5}
                        justifyContent={"center"}
                      >
                        <Text padding={2}>{modifierProperty.name}:</Text>
                        {themePropertyUi}
                      </HStack>
                    );
                  })}
              </>
            }
          ></Draggable>
        );
      })}
    </Box>
  );
};

export default ThemeModifiersEditor;
