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
  Portal,
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
  popoverStyle?: React.CSSProperties | undefined;
  buttonStyle?: React.CSSProperties | undefined;
}

const EditButton = ({
  children,
  popoverStyle = undefined,
  buttonStyle = undefined,
}: Props) => {
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
              style={buttonStyle}
            />
          </PopoverTrigger>
          {/* The popover content must live in a portal (not trapped inside its trigger's stacking
              context, e.g. the segment button's backdrop-filter) so it can float above sibling UI
              like the color picker and the theme-modifier cards. Z-index/opacity are forced globally
              in App.css so every edit popover renders on top and opaque. */}
          <Portal>
            <PopoverContent cursor={"default"} padding={1} style={popoverStyle}>
              <FocusLock autoFocus={true} persistentFocus={true}>
                <PopoverArrow />
                <PopoverCloseButton />
                <PopoverBody>
                  {children(isOpen, onOpen, onClose, firstFieldRef)}
                </PopoverBody>
              </FocusLock>
            </PopoverContent>
          </Portal>
        </>
      )}
    </Popover>
  );
};

export default EditButton;
