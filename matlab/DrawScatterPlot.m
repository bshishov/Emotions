function DrawScatterPlot( X, idx )
figure;

hold all
plotmatrix(X(idx==1,:),'*r')
%plotmatrix(X(idx==2,:),'.g')
%plotmatrix(X(idx==3,:),'+b')
hold off
end

