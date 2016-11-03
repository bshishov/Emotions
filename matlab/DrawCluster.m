function DrawCluster( X, idx, C, p1, p2 )

figure;

plot(X(idx==1,p1),X(idx==1,p2),'r.','MarkerSize',20)
hold on
plot(X(idx==2,p1),X(idx==2,p2),'b.','MarkerSize',20)
hold on
plot(X(idx==3,p1),X(idx==3,p2),'g.','MarkerSize',20)
hold on
plot(C(:,p1),C(:,p2),'kx','MarkerSize',15,'LineWidth',3)
title('Expirements classification')
xlabel('t critical 1') % x-axis label
ylabel('t critical 2') % y-axis label
legend('Class 1','Class 2','Class 3','Class cetners')
hold off

end

