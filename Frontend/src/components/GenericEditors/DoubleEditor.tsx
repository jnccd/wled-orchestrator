import {
  Box,
  Slider,
  SliderMark,
  SliderTrack,
  SliderFilledTrack,
  SliderThumb,
} from "@chakra-ui/react";
import { useState } from "react";
import { readProperty, writeProperty } from "../../utils/untypedPropertyAccess";

interface Props {
  editingObject: object;
  propertyName: string;
  onChange: (newEditingObject: object) => void;
}

const DoubleEditor = ({ editingObject, propertyName, onChange }: Props) => {
  const [refreshBool, refresh] = useState(false);

  const propertyValue = readProperty(editingObject, propertyName);

  return (
    <Box p={4} pt={6} width={"100%"} minWidth={"100px"}>
      <Slider
        aria-label="slider-ex-6"
        onChange={(val) => {
          writeProperty(editingObject, propertyName, val);
          refresh(!refreshBool);
        }}
        onChangeEnd={() => onChange(editingObject)}
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

export default DoubleEditor;
