import { Box, HStack, Text } from "@chakra-ui/react";
import { WledOrchState } from "../../hooks/useWledOrchState";
import { components } from "../../types/api";
import EditNameButton from "../EditNameButton";
import WledSegmentViewer from "./WledSegmentViewer";

const ledSegmentClassName = "led-segment-group";

interface Props {
  selectedGroupId: string;
  setSelectedGroupId: (newId: string) => void;
  refreshWledGroupViewer: () => void;
  wledOrchState: WledOrchState;
  group: components["schemas"]["LedSegmentGroup"];
}

const WledSegmentGroupViewer = ({
  selectedGroupId,
  setSelectedGroupId,
  refreshWledGroupViewer,
  wledOrchState,
  group: g,
}: Props) => {
  return (
    <Box
      key={g.id}
      className={ledSegmentClassName + " " + (g.id ?? "error-id-less-group")}
      borderWidth={selectedGroupId === g.id ? "3px" : "1px"}
      borderRadius="lg"
      margin={2}
      padding="2px"
      width={"fit-content"}
      transition="border .1s"
      onClick={() => {
        if (g.id) setSelectedGroupId(g.id);
      }}
    >
      <HStack justifyContent={"center"} gap={0}>
        <Text
          fontWeight={selectedGroupId === g.id ? "bold" : "normal"}
          margin={2}
        >
          {g.name}
        </Text>
        <EditNameButton></EditNameButton>
      </HStack>
      <HStack gap={0}>
        {g.ledSegments &&
          g.ledSegments.map((s) => (
            <WledSegmentViewer
              ledSegmentClassName={ledSegmentClassName}
              wledOrchState={wledOrchState}
              refreshWledGroupViewer={refreshWledGroupViewer}
              segment={s}
              key={s.readonlyId}
            ></WledSegmentViewer>
          ))}
      </HStack>
    </Box>
  );
};

export default WledSegmentGroupViewer;
