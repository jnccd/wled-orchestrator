import { Box, HStack, VStack } from "@chakra-ui/react";
import { useState } from "react";
import "./TimeEditor.css";
import { readProperty, writeProperty } from "../../utils/untypedPropertyAccess";
import { TimeInput } from "@heroui/react";
import { parseTime } from "@internationalized/date";

interface Props {
  editingObject: object;
  propertyName: string;
  onChange: (newEditingObject: object) => void;
}

const TimeEditor = ({ editingObject, propertyName, onChange }: Props) => {
  const [refreshBool, refresh] = useState(false);

  const propertyValue = readProperty(editingObject, propertyName);

  return (
    <HStack
      p={2}
      px={3}
      backgroundColor={"#3182CE"}
      borderRadius="10px"
      cursor={"default"}
    >
      <TimeInput
        fullWidth={true}
        defaultValue={parseTime(`${propertyValue}`)}
        granularity="minute"
        hourCycle={24}
        onChange={(val) => {
          writeProperty(editingObject, propertyName, val?.toString());
          onChange(editingObject);
          refresh(!refreshBool);
        }}
      />
    </HStack>
  );
};

export default TimeEditor;
