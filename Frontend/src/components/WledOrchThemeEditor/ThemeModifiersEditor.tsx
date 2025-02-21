import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addThemeModifier,
  deleteThemeModifier,
  getThemeTypes,
  getWledOrchState,
  setThemeModifier,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import {
  Text,
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
import ThemePaneHeader from "./ThemePaneHeader";
import TimeEditor from "../GenericEditors/TimeEditor";

const firstCharToLowerCase = (
  text: string | null | undefined
): string | undefined =>
  !text || text === "" ? undefined : text[0]?.toLowerCase() + text.slice(1);

const ThemeModifiersEditor = () => {
  const draggableClassName = "theme-modifier-draggable";

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
  const deleteModifierMutation = useMutation({
    mutationFn: deleteThemeModifier,
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
      <ThemePaneHeader>Modifiers</ThemePaneHeader>
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
                  index: null,
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
            className={`${draggableClassName} ${modifier.id}`}
            onDragEnd={(_, mousePos) => {
              const draggables =
                document.getElementsByClassName(draggableClassName);
              const draggablesOverMousePos = [...draggables]
                .map((elem, index) => [elem, index])
                .filter((x) => {
                  const bounds = (x[0] as Element).getBoundingClientRect();
                  return bounds.top + bounds.height / 2 < mousePos[1];
                });
              deleteModifierMutation.mutate({
                groupId: selectedGroup?.id ?? "",
                modifierId: modifier?.id ?? "",
              });
              addModifierMutation.mutate({
                groupId: selectedGroup?.id ?? "",
                newModifier: modifier,
                index: draggablesOverMousePos.length,
              });
            }}
            children={
              <>
                <Text key={modifier.id + "text"} as={"b"}>
                  {modifier.typeName} Modifier
                </Text>
                {themeTypesQuery.data?.modifiers
                  ?.filter(
                    (x) =>
                      x.typeDiscriminator === readProperty(modifier, "$type")
                  )[0]
                  .properties?.map((modifierProperty, index) => {
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
                          key={modifier.id + "editor" + index}
                          editingObject={modifier}
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
                          key={modifier.id + "editor" + index}
                          editingObject={modifier}
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
                    } else if (modifierProperty.type === "TimeSpan") {
                      themePropertyUi = (
                        <TimeEditor
                          key={modifier.id + "editor" + index}
                          editingObject={modifier}
                          propertyName={propertyName}
                          onChange={(object: any) =>
                            changeModifierMutation.mutate({
                              groupId: selectedGroup?.id ?? "",
                              modifierId: modifier.id ?? "",
                              newModifier: object,
                            })
                          }
                        ></TimeEditor>
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
                        paddingTop={3}
                        justifyContent={"center"}
                      >
                        <Text padding={2}>{modifierProperty.name}:</Text>
                        {themePropertyUi}
                      </HStack>
                    );
                  })}
                <Button
                  colorScheme="red"
                  marginTop={5}
                  marginBottom={2}
                  onClick={() => {
                    deleteModifierMutation.mutate({
                      groupId: selectedGroup.id ?? "",
                      modifierId: modifier.id ?? "",
                    });
                  }}
                >
                  Delete
                </Button>
              </>
            }
          ></Draggable>
        );
      })}
    </Box>
  );
};

export default ThemeModifiersEditor;
