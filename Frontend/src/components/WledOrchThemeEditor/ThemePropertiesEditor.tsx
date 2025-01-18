import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getThemeTypes,
  getWledOrchState,
  setGroupTheme,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";
import {
  Text,
  HStack,
  Box,
  Slider,
  SliderMark,
  SliderTrack,
  SliderFilledTrack,
  SliderThumb,
  Heading,
} from "@chakra-ui/react";
import useSelectedGroupStore from "../../hooks/useLocalStore";
import { useState } from "react";
import { readProperty } from "../../utils/untypedPropertyAccess";
import ThemePropertyColorEditor from "./ThemePropertyColorEditor";
import ThemePropertyDoubleEditor from "./ThemePropertyDoubleEditor";

const firstCharToLowerCase = (
  text: string | null | undefined
): string | undefined =>
  !text || text === "" ? undefined : text[0]?.toLowerCase() + text.slice(1);

const ThemePropertiesEditor = () => {
  const [refreshBool, refresh] = useState(false);

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
    <>
      <Heading padding={4} paddingTop={7} fontSize={30}>
        Theme Properties:
      </Heading>
      {themeTypesQuery.data?.themes
        ?.filter(
          (themeTypes) => themeTypes.name === selectedGroup?.theme?.typeName
        )[0]
        .properties?.map((themeTypeProperty) => {
          const propertyName = firstCharToLowerCase(themeTypeProperty.name);
          if (!propertyName) {
            return <Text>Invalid property name!</Text>;
          }
          const propertyValue = readProperty(
            selectedGroup?.theme,
            propertyName
          );

          var themePropertyUi: JSX.Element = <></>;
          if (themeTypeProperty.type === "Color") {
            themePropertyUi = (
              <ThemePropertyColorEditor
                propertyName={propertyName}
                propertyValue={propertyValue}
              ></ThemePropertyColorEditor>
            );
          } else if (themeTypeProperty.type === "Double") {
            themePropertyUi = (
              <ThemePropertyDoubleEditor
                propertyName={propertyName}
                propertyValue={propertyValue}
              ></ThemePropertyDoubleEditor>
            );
          } else {
            themePropertyUi = (
              <Text>{themeTypeProperty.type} input field is undefined!</Text>
            );
          }

          return (
            <HStack width={"100%"} key={propertyName} padding={3}>
              <Text padding={2}>{themeTypeProperty.name}:</Text>
              {themePropertyUi}
            </HStack>
          );
        })}
    </>
  );
};

export default ThemePropertiesEditor;
