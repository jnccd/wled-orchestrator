import { EditIcon } from "@chakra-ui/icons";
import {
  FocusLock,
  IconButton,
  Input,
  Popover,
  PopoverArrow,
  PopoverBody,
  PopoverCloseButton,
  PopoverContent,
  PopoverTrigger,
  useDisclosure,
} from "@chakra-ui/react";
import React from "react";

interface Props {
  defaultValue?: string;
  onSubmit?: (newName: string) => void;
}

const EditNameButton = ({ onSubmit, defaultValue }: Props) => {
  const { onOpen, onClose, isOpen } = useDisclosure();
  const firstFieldRef = React.useRef(null);

  return (
    <Popover
      isOpen={isOpen}
      onOpen={onOpen}
      onClose={onClose}
      initialFocusRef={firstFieldRef}
    >
      {({ onClose }) => (
        <>
          <PopoverTrigger>
            <IconButton
              className="consumes-click"
              size="sm"
              icon={<EditIcon />}
              aria-label={""}
            />
          </PopoverTrigger>
          <PopoverContent cursor={"default"}>
            <FocusLock autoFocus={true} persistentFocus={true}>
              <PopoverArrow />
              <PopoverCloseButton />
              <PopoverBody padding={6}>
                <Input
                  marginTop={2}
                  ref={firstFieldRef}
                  defaultValue={defaultValue}
                  //id={popoverInputId}
                  onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                    if (e.key == "Enter") {
                      const inputVal = (e.target as HTMLInputElement).value;
                      if (onSubmit) onSubmit(inputVal);
                      onClose();
                    }
                  }}
                />
              </PopoverBody>
            </FocusLock>
          </PopoverContent>
        </>
      )}
    </Popover>
  );
};

export default EditNameButton;
