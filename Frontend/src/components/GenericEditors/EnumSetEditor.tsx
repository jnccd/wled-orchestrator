import { useState } from "react";
import { readProperty, writeProperty } from "../../utils/untypedPropertyAccess";
import {
  CompoundTypeInfo,
  GenerateFrontendFormData,
} from "../../hooks/useWledOrchApi";
import { Checkbox, CheckboxGroup, HStack, VStack } from "@chakra-ui/react";

interface Props {
  editingObject: object;
  propertyName: string;
  settings: GenerateFrontendFormData;
  compoundTypeInfo: CompoundTypeInfo | null;
  onChange: (newEditingObject: object) => void;
}

const EnumSetEditor = ({
  editingObject,
  propertyName,
  compoundTypeInfo,
  onChange,
}: Props) => {
  const [refreshBool, refresh] = useState(false);

  var propertyValue = readProperty(editingObject, propertyName) as string[];

  return (
    <VStack alignItems={""}>
      <CheckboxGroup colorScheme="green" defaultValue={propertyValue}>
        {compoundTypeInfo?.validEnumValues?.map((enumValue) => (
          <HStack>
            <Checkbox
              className="consumes-click"
              value={enumValue}
              checked={propertyValue.includes(enumValue)}
              onChange={(e) => {
                if (e.target.checked) {
                  propertyValue.push(enumValue);
                } else {
                  propertyValue = propertyValue.filter((x) => x !== enumValue);
                }
                writeProperty(editingObject, propertyName, propertyValue);
                refresh(!refreshBool);
                onChange(editingObject);
              }}
            >
              {enumValue}
            </Checkbox>
          </HStack>
        ))}
      </CheckboxGroup>
    </VStack>
  );
};

export default EnumSetEditor;
