import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getWledOrchState,
  setGroupTheme,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";
import useSelectedGroupStore from "../../hooks/useLocalStore";
import {
  Box,
  Slider,
  SliderMark,
  SliderTrack,
  SliderFilledTrack,
  SliderThumb,
} from "@chakra-ui/react";
import { useState } from "react";
import { writeProperty } from "../../utils/untypedPropertyAccess";

interface Props {
  propertyName: string;
  propertyValue: any;
}

const ThemePropertyColorEditor = ({ propertyName, propertyValue }: Props) => {
  const [refreshBool, refresh] = useState(false);

  // React Query setup
  const queryClient = useQueryClient();
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
    <Box p={4} pt={6} width={"100%"}>
      <Slider
        aria-label="slider-ex-6"
        onChange={(val) => {
          writeProperty(selectedGroup?.theme, propertyName, val);
          refresh(!refreshBool);
        }}
        onChangeEnd={() =>
          changeThemeMutation.mutate({
            groupId: selectedGroup?.id ?? "",
            newTheme: selectedGroup?.theme,
          })
        }
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
  );
};

export default ThemePropertyColorEditor;
