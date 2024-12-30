import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getWledOrchState,
  setActivated,
  wledOrchStateQueryKey,
} from "../hooks/useWledOrchState";
import { IoPower } from "react-icons/io5";

const WledOrchActivationSwitch = () => {
  // React Query setup
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const setActivatedMutation = useMutation({
    mutationFn: setActivated,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });
  const activated = query.data?.activated;

  return (
    <button
      style={{ border: "none", background: "none", outline: "none" }}
      onClick={(_) => setActivatedMutation.mutate({ newActivated: !activated })}
    >
      {!query.isSuccess && <IoPower size={25} color="gray" />}
      {query.isSuccess && activated && <IoPower size={25} color="green" />}
      {query.isSuccess && !activated && <IoPower size={25} color="red" />}
    </button>
  );
};

export default WledOrchActivationSwitch;
