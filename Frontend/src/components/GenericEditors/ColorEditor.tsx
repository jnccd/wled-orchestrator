import { Colorful, ColorResult } from "@uiw/react-color";
import { useState } from "react";
import { readProperty, writeProperty } from "../../utils/untypedPropertyAccess";

interface Props {
  editingObject: object;
  propertyName: string;
  onChange: (newEditingObject: object) => void;
}

const ColorEditor = ({ editingObject, propertyName, onChange }: Props) => {
  const [refreshBool, refresh] = useState(false);

  const propertyValue = readProperty(editingObject, propertyName);

  return (
    <Colorful
      color={{
        h: propertyValue.h,
        s: propertyValue.s,
        v: propertyValue.v,
        a: 1,
      }}
      onChange={(colorRes: ColorResult) => {
        writeProperty(editingObject, propertyName, colorRes.hsv);
        refresh(!refreshBool);
      }}
      onMouseUp={() => {
        onChange(editingObject);
      }}
      disableAlpha
    ></Colorful>
  );
};

export default ColorEditor;
