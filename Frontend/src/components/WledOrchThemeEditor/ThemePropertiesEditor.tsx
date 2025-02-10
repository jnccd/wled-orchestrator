import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getThemeTypes,
  getWledOrchState,
  setGroupTheme,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { Text, HStack, Heading, Box } from "@chakra-ui/react";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import { readProperty } from "../../utils/untypedPropertyAccess";
import ColorEditor from "../GenericEditors/ColorEditor";
import DoubleEditor from "../GenericEditors/DoubleEditor";

const firstCharToLowerCase = (
  text: string | null | undefined
): string | undefined =>
  !text || text === "" ? undefined : text[0]?.toLowerCase() + text.slice(1);

const ThemePropertiesEditor = () => {
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

  return (
    <Box display="flex" flexDirection="column">
      <Heading padding={4} fontSize={30}>
        Theme Properties:
      </Heading>
      {themeTypesQuery.data?.themes
        ?.filter(
          (themeTypes) =>
            themeTypes.typeDiscriminator ===
            readProperty(selectedGroup?.theme, "$type")
        )[0]
        .properties?.map((themeTypeProperty) => {
          const propertyName = firstCharToLowerCase(themeTypeProperty.name);
          if (!propertyName) {
            return <Text>Invalid property name!</Text>;
          }

          let themePropertyUi: JSX.Element = <></>;
          if (themeTypeProperty.type === "ColorHsv") {
            themePropertyUi = (
              <ColorEditor
                editingObject={selectedGroup?.theme ?? {}}
                propertyName={propertyName}
                onChange={(object: any) =>
                  changeThemeMutation.mutate({
                    groupId: selectedGroup?.id ?? "",
                    newTheme: object,
                  })
                }
              ></ColorEditor>
            );
          } else if (themeTypeProperty.type === "Double") {
            themePropertyUi = (
              <DoubleEditor
                editingObject={selectedGroup?.theme ?? {}}
                propertyName={propertyName}
                onChange={(object: any) =>
                  changeThemeMutation.mutate({
                    groupId: selectedGroup?.id ?? "",
                    newTheme: object,
                  })
                }
              ></DoubleEditor>
            );
          } else {
            themePropertyUi = (
              <Text>{themeTypeProperty.type} input field is undefined!</Text>
            );
          }

          return (
            <HStack
              width={"100%"}
              key={propertyName}
              padding={5}
              justifyContent={"center"}
            >
              <Text padding={2}>{themeTypeProperty.name}:</Text>
              {themePropertyUi}
            </HStack>
          );
        })}
    </Box>
  );
};

export default ThemePropertiesEditor;
