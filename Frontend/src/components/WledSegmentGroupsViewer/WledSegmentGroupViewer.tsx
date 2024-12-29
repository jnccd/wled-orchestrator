import { Box, HStack, Text } from "@chakra-ui/react";
import EditNameButton from "../EditNameButton";
import WledSegmentViewer from "./WledSegmentViewer";
import useSelectedGroupStore from "../../hooks/useLocalStore";
import { LedSegmentGroup } from "../../hooks/useWledOrchState";

const ledSegmentClassName = "led-segment-group";

interface Props {
  group: LedSegmentGroup;
}

const WledSegmentGroupViewer = ({ group: g }: Props) => {
  const selectedGroupStore = useSelectedGroupStore();
  selectedGroupStore.initialize();

  return (
    <Box
      key={g.id}
      className={ledSegmentClassName + " " + (g.id ?? "error-id-less-group")}
      borderWidth={selectedGroupStore.selectedGroup === g.id ? "3px" : "1px"}
      borderRadius="lg"
      margin={2}
      padding="2px"
      width={"fit-content"}
      transition="border .1s"
      onClick={() => {
        if (g.id) selectedGroupStore.selectNew(g.id);
      }}
    >
      <HStack justifyContent={"center"} gap={0} padding={1}>
        <Text
          fontWeight={
            selectedGroupStore.selectedGroup === g.id ? "bold" : "normal"
          }
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
              segment={s}
              key={s.readonlyId}
            ></WledSegmentViewer>
          ))}
      </HStack>
    </Box>
  );
};

export default WledSegmentGroupViewer;
