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
  Center,
  VStack,
  Slider,
  SliderMark,
  SliderTrack,
  SliderFilledTrack,
  SliderThumb,
} from "@chakra-ui/react";
import { ChevronDownIcon } from "@chakra-ui/icons";
import useSelectedGroupStore from "../../hooks/useLocalStore";
import { Colorful, ColorResult, rgbaToHsva } from "@uiw/react-color";

const readProperty = (obj: any, prop: string): any => {
  return obj[prop];
};

const writeProperty = (obj: any, prop: string, newValue: any): void => {
  obj[prop] = newValue;
};

const firstCharToLowerCase = (
  text: string | null | undefined
): string | undefined =>
  !text || text === "" ? undefined : text[0]?.toLowerCase() + text.slice(1);

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
      </HStack>
      <Box display="flex" flexDirection="column">
        {!selectedGroup?.theme ? (
          <Text>-</Text>
        ) : (
          <Center>
            <VStack>
              {themeTypesQuery.data?.themes
                ?.filter(
                  (themeTypes) =>
                    themeTypes.name === selectedGroup?.theme?.typeName
                )[0]
                .properties?.map((themeTypeProperty) => {
                  const propertyName = firstCharToLowerCase(
                    themeTypeProperty.name
                  );
                  if (!propertyName) {
                    return <Text>Invalid property name!</Text>;
                  }
                  const propertyValue = readProperty(
                    selectedGroup?.theme,
                    propertyName
                  );
                  if (themeTypeProperty.type === "Color") {
                    return (
                      <HStack>
                        <Text>{themeTypeProperty.name}:</Text>
                        <Colorful
                          color={rgbaToHsva({
                            r: propertyValue.r,
                            g: propertyValue.g,
                            b: propertyValue.b,
                            a: 1,
                          })}
                          onChange={(colorRes: ColorResult) => {
                            writeProperty(
                              selectedGroup?.theme,
                              propertyName,
                              colorRes.rgb
                            );
                            changeThemeMutation.mutate({
                              groupId: selectedGroup.id ?? "",
                              newTheme: selectedGroup?.theme,
                            });
                          }}
                          disableAlpha
                        ></Colorful>
                      </HStack>
                    );
                  } else if (themeTypeProperty.type === "Double") {
                    return (
                      <HStack width={"100%"}>
                        <Text>{themeTypeProperty.name}:</Text>
                        <Box p={4} pt={6} width={"100%"}>
                          <Slider
                            aria-label="slider-ex-6"
                            onChange={(val) => {
                              writeProperty(
                                selectedGroup?.theme,
                                propertyName,
                                val
                              );
                              changeThemeMutation.mutate({
                                groupId: selectedGroup.id ?? "",
                                newTheme: selectedGroup?.theme,
                              });
                            }}
                            defaultValue={propertyValue}
                            min={0}
                            max={300}
                          >
                            <SliderMark
                              value={propertyValue}
                              textAlign="center"
                              bg="blue.500"
                              color="white"
                              mt="-10"
                              ml="-5"
                              w="12"
                              borderRadius={"6px"}
                            >
                              {propertyValue}
                            </SliderMark>
                            <SliderTrack>
                              <SliderFilledTrack />
                            </SliderTrack>
                            <SliderThumb />
                          </Slider>
                        </Box>
                      </HStack>
                    );
                  } else {
                    <HStack justifyContent={"center"}>
                      <Text>{themeTypeProperty.name}:</Text>
                      <Text>{themeTypeProperty.type} input field</Text>
                    </HStack>;
                  }
                })}
            </VStack>
          </Center>
        )}
      </Box>
    </>
  );
};

export default WledOrchThemeEditor;
