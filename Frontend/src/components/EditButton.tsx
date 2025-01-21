import { EditIcon } from "@chakra-ui/icons";
import {
  FocusLock,
  IconButton,
  Popover,
  PopoverArrow,
  PopoverBody,
  PopoverCloseButton,
  PopoverContent,
  PopoverTrigger,
  useDisclosure,
} from "@chakra-ui/react";
import React, { ReactNode } from "react";

interface Props {
  children: (
    isOpen: boolean,
    onOpen: () => void,
    onClose: () => void,
    firstFieldRef: React.MutableRefObject<null>
  ) => ReactNode;
}

const EditButton = ({ children }: Props) => {
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
          <PopoverContent cursor={"default"} padding={1}>
            <FocusLock autoFocus={true} persistentFocus={true}>
              <PopoverArrow />
              <PopoverCloseButton />
              <PopoverBody>
                {children(isOpen, onOpen, onClose, firstFieldRef)}
              </PopoverBody>
            </FocusLock>
          </PopoverContent>
        </>
      )}
    </Popover>
  );
};

export default EditButton;
