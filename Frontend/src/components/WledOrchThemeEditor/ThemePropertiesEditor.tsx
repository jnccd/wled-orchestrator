import { useQuery } from "@tanstack/react-query";
import {
  getThemeTypes,
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { Text, HStack, Heading } from "@chakra-ui/react";
import { useSelectedGroupStore } from "../../hooks/useLocalStore";
import ThemePropertyColorEditor from "./ThemePropertyColorEditor";
import ThemePropertyDoubleEditor from "./ThemePropertyDoubleEditor";
import { readProperty } from "../../utils/untypedPropertyAccess";

const firstCharToLowerCase = (
  text: string | null | undefined
): string | undefined =>
  !text || text === "" ? undefined : text[0]?.toLowerCase() + text.slice(1);

const ThemePropertiesEditor = () => {
  // React Query setup
  const themeTypesQuery = useQuery({
    queryKey: [],
    queryFn: getThemeTypes,
  });
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

          var themePropertyUi: JSX.Element = <></>;
          if (themeTypeProperty.type === "Color") {
            themePropertyUi = (
              <ThemePropertyColorEditor
                propertyName={propertyName}
              ></ThemePropertyColorEditor>
            );
          } else if (themeTypeProperty.type === "Double") {
            themePropertyUi = (
              <ThemePropertyDoubleEditor
                propertyName={propertyName}
              ></ThemePropertyDoubleEditor>
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
    </>
  );
};

export default ThemePropertiesEditor;
