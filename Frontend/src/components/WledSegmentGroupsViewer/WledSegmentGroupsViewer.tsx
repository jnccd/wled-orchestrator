import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";
import { HStack } from "@chakra-ui/react";
import WledSegmentGroupViewer from "./WledSegmentGroupViewer";
import { useQuery } from "@tanstack/react-query";

interface Props {}

const WledSegmentGroupsViewer = ({}: Props) => {
  const query = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  return (
    !query.isPending && (
      <HStack justifyContent="center" width="100%">
        {query.data?.groups?.map((g) => (
          <WledSegmentGroupViewer group={g} key={g.id}></WledSegmentGroupViewer>
        ))}
      </HStack>
    )
  );
};

export default WledSegmentGroupsViewer;
