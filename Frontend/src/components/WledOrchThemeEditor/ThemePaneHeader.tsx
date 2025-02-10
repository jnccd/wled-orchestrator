import { Heading } from "@chakra-ui/react";
import { ReactNode } from "react";

interface Props {
  children: ReactNode;
}

const ThemePaneHeader = ({ children }: Props) => {
  return (
    <Heading fontSize={24} padding={4}>
      {children}
    </Heading>
  );
};

export default ThemePaneHeader;
