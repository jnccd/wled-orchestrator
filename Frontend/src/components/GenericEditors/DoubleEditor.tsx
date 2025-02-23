import {
  Text,
  Box,
  Slider,
  SliderMark,
  SliderTrack,
  SliderFilledTrack,
  SliderThumb,
  NumberInput,
  NumberInputField,
  NumberInputStepper,
  NumberIncrementStepper,
  NumberDecrementStepper,
} from "@chakra-ui/react";
import { useState } from "react";
import { readProperty, writeProperty } from "../../utils/untypedPropertyAccess";
import { GenerateFrontendFormData } from "../../hooks/useWledOrchApi";
import useThrottle from "../../hooks/useThrottle";

interface Props {
  editingObject: object;
  propertyName: string;
  settings: GenerateFrontendFormData;
  onChange: (newEditingObject: object) => void;
}

const DoubleEditor = ({
  editingObject,
  propertyName,
  settings,
  onChange,
}: Props) => {
  const [refreshBool, refresh] = useState(false);

  const propertyValue = readProperty(editingObject, propertyName);

  if (!settings?.inputType || settings.inputType === "slider") {
    return (
      <Box p={4} pt={8} pb={1} width={"100%"} minWidth={"100px"}>
        <Slider
          aria-label="slider-ex-6"
          onChange={(val) => {
            writeProperty(editingObject, propertyName, val);
            refresh(!refreshBool);
          }}
          onChangeEnd={() => onChange(editingObject)}
          defaultValue={propertyValue}
          min={settings?.minValue}
          max={settings?.maxValue}
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
  } else if (settings.inputType === "number-input") {
    const throttle = useThrottle();

    return (
      <NumberInput
        allowMouseWheel
        defaultValue={propertyValue}
        min={settings?.minValue}
        max={settings?.maxValue}
        onChange={(_valString, valNum) => {
          throttle(() => {
            writeProperty(editingObject, propertyName, valNum);
            refresh(!refreshBool);
            onChange(editingObject);
          }, 1500);
        }}
        width="100px"
      >
        <NumberInputField />
        <NumberInputStepper>
          <NumberIncrementStepper />
          <NumberDecrementStepper />
        </NumberInputStepper>
      </NumberInput>
    );
  } else {
    return <Text>Invalid number input type :/</Text>;
  }
};

export default DoubleEditor;
