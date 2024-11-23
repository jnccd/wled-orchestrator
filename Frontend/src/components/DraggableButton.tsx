import { Button } from "@chakra-ui/react";
import React, { useEffect, useState } from "react";

interface Props {
  ownId: string;
  buttonName: string;
}

const DraggableButton = ({ ownId, buttonName }: Props) => {
  const [dragging, setDragging] = useState(false);
  const [draggingStartPos, setDraggingStartPos] = useState([0, 0]);
  const dragY = false;

  const dragMouseDown = (e: React.MouseEvent) => {
    console.log("dragMouseDown");
    e.preventDefault();

    var thisInDocument = document.getElementById(ownId);
    if (thisInDocument === null) {
      console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    setDraggingStartPos([
      e.clientX - (parseInt(thisInDocument.style.left) || 0),
      e.clientY - (parseInt(thisInDocument.style.top) || 0),
    ]);
    thisInDocument.style.zIndex = "10";
    console.log(draggingStartPos);
    setDragging(true);
  };

  const dragMouseMove = (e: React.MouseEvent) => {
    console.log("dragMouseMove, " + dragging + ownId);
    if (!dragging) {
      return;
    }

    e.preventDefault();

    var thisInDocument = document.getElementById(ownId);
    if (thisInDocument === null) {
      console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    thisInDocument.style.left = e.clientX - draggingStartPos[0] + "px";
    if (dragY) {
      thisInDocument.style.top = e.clientY - draggingStartPos[1] + "px";
    }
  };

  const dragMouseUp = () => {
    console.log("dragMouseUp");
    var thisInDocument = document.getElementById(ownId);
    if (thisInDocument === null) {
      console.log("dragMouseMove, cant find thisInDocument");
      return;
    }

    thisInDocument.style.zIndex = "1";
    setDragging(false);
  };

  return (
    <Button
      id={ownId}
      position={"relative"}
      className="wledServerButton"
      //transition="all 0.5s"
      margin={2}
      size="lg"
      cursor={"move"}
      onMouseDown={dragMouseDown}
      onMouseMove={dragMouseMove}
      onMouseUp={dragMouseUp}
    >
      {buttonName}
    </Button>
  );
};

export default DraggableButton;
